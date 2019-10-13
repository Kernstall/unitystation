﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using PathFinding;
using UnityScript.Macros;

public class MobPathFinder : MonoBehaviour
{
	protected RegisterTile registerTile;
	protected Matrix matrix => registerTile.Matrix;
	protected CustomNetTransform cnt;

	protected bool isServer;

	public bool performingDecision;

	public float tickRate = 1f;
	private float tickWait;
	private float timeOut;

	private bool movingToTile;
	private bool arrivedAtTile;

	//For OnDrawGizmos
	private List<Node> debugPath;

	public enum Status
	{
		idle,
		searching,
		followingPath
	}

	public Status status;
	private Node startNode;
	private Node goalNode;
	private bool pathFound;
	PriorityQueue<Node> frontierNodes;
	private List<Node> exploredNodes = new List<Node>();
	private Dictionary<Vector2Int, Node> allNodes = new Dictionary<Vector2Int, Node>();

	private bool isComplete = false;

	private void Awake()
	{
		registerTile = GetComponent<RegisterTile>();
		cnt = GetComponent<CustomNetTransform>();
	}

	public virtual void OnEnable()
	{
		//only needed for starting via a map scene through the editor:
		if (CustomNetworkManager.Instance == null) return;

		if (CustomNetworkManager.Instance._isServer)
		{
			cnt.OnTileReached().AddListener(OnTileReached);
			isServer = true;
		}
	}

	public virtual void OnDisable()
	{
		if (isServer)
		{
			cnt.OnTileReached().RemoveListener(OnTileReached);
		}
	}

	void OnDrawGizmos()
	{
		if (debugPath != null)
		{
			for (var i = 1; i < debugPath.Count; ++i)
			{
				var from = new Vector3(debugPath[i - 1].position.x + 1, debugPath[i - 1].position.y + 1, 0);
				var to = new Vector3(debugPath[i].position.x + 1, debugPath[i].position.y + 1, 0);
				Gizmos.DrawLine(from, to);
			}
		}
	}

	///<summary>
	/// Use local positions related to the matrix the object is on
	///</summary>
	public List<Node> FindNewPath(Vector2Int startPos, Vector2Int targetPos)
	{
		if (startPos == targetPos) return null;

		pathFound = false;

		startNode = new Node
		{
			position = startPos,
			distanceTraveled = 0
		};

		goalNode = new Node
		{
			position = targetPos
		};

		isComplete = false;

		frontierNodes = new PriorityQueue<Node>();
		frontierNodes.Enqueue(startNode);
		exploredNodes.Clear();
		allNodes.Clear();
		allNodes.Add(startNode.position, startNode);
		allNodes.Add(goalNode.position, goalNode);
		return SearchForRoute();
	}

	List<Node> SearchForRoute()
	{
		status = Status.searching;
		int timeOut = 0;
		while (!isComplete)
		{
			timeOut++;
			if (timeOut > 500)
			{
				isComplete = true;
				//This could be because you are trying to use a goal node that is inside a wall or the path was blocked
				Logger.LogError("Pathing finding could not find a path where one was expected to be found", Category.Movement);
				return null;
			}

			if (frontierNodes.Count > 0)
			{
				Node currentNode = frontierNodes.Dequeue();

				if (currentNode.neighbors == null)
				{
					FindNeighbours(currentNode);
				}

				if (!exploredNodes.Contains(currentNode))
				{
					exploredNodes.Add(currentNode);
				}

				ExpandFrontierAStar(currentNode);

				if (pathFound)
				{
					isComplete = true;
					List<Node> path = new List<Node>();

					Node nextNode = goalNode;

					do
					{
						path.Add(nextNode);
						nextNode = nextNode.previous;
					} while (nextNode != null);

					path.Reverse();

					return path;
				}
			}
			else
			{
				isComplete = true;
				return null;
			}
		}

		status = Status.idle;
		return null;
	}

	void FindNeighbours(Node currentNode)
	{
		Vector2Int startPos = currentNode.position + new Vector2Int(-1, 1);
		List<Node> newNeighbours = new List<Node>(8);

		for (int i = 0; i < 3; i++)
		{
			for (int y = 0; y < 3; y++)
			{
				Vector2Int searchPos = new Vector2Int(startPos.x + i, startPos.y - y);
				if (searchPos == currentNode.position)
				{
					continue;
				}

				if (!allNodes.ContainsKey(searchPos))
				{
					Node newNode = new Node
					{
						position = searchPos
					};
					allNodes.Add(newNode.position, newNode);
					newNeighbours.Add(newNode);
				}
				else
				{
					newNeighbours.Add(allNodes[searchPos]);
				}
			}
		}

		currentNode.neighbors = newNeighbours;
	}

	private void ExpandFrontierAStar(Node node)
	{
		if (node == null)
			return;

		for (int i = 0; i < node.neighbors.Count; i++)
		{
			Node neighbor = node.neighbors[i];

			if (exploredNodes.Contains(neighbor))
				continue;

			RefreshNodeType(neighbor);

			if (node.neighbors[i].nodeType == PathFinding.NodeType.Blocked)
				continue;

			float distanceToNeighbor = GetNodeDistance(node, neighbor);
			float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int) node.nodeType;
			uint newPriority = (uint) (100 * (newDistanceTraveled + GetNodeDistance(neighbor, goalNode)));

			if (neighbor.priority > newPriority)
			{
				neighbor.previous = node;
				neighbor.distanceTraveled = newDistanceTraveled;
				neighbor.priority = newPriority;
				if (node == goalNode)
				{
					pathFound = true;
				}
				else
				{
					frontierNodes.Remove(neighbor); // Re-sort if the node existed already.
					frontierNodes.Enqueue(neighbor);
				}
			}
		}
	}

	/// <summary>
	/// Start following a given path
	/// </summary>
	protected void FollowPath(List<Node> path)
	{
		status = Status.followingPath;
	//	debugPath = path;
		StartCoroutine(PerformFollowPath(path));
	}

	IEnumerator PerformFollowPath(List<Node> path)
	{
		int node = 1;

		while (node < path.Count)
		{
			if (!movingToTile)
			{
				var dir = path[node].position - Vector2Int.RoundToInt(transform.localPosition);
				if (!registerTile.Matrix.IsPassableAt(registerTile.LocalPositionServer + (Vector3Int) dir, true))
				{
					var dC = registerTile.Matrix.GetFirst<DoorController>(registerTile.LocalPositionServer + (Vector3Int) dir, true);
					if (dC != null)
					{
						dC.TryOpen(gameObject);
						yield return WaitFor.Seconds(1f);
					}
					else
					{
						ResetMovingValues();
						FollowCompleted();
						Logger.Log("Path following timed out. Something must be in the way", Category.Movement);
						yield break;
					}
				}

				cnt.Push(dir);
				movingToTile = true;
			}
			else
			{
				if (arrivedAtTile)
				{
					movingToTile = false;
					arrivedAtTile = false;
					timeOut = 0f;
					node++;
				}
				else
				{
					//Mob has 5 seconds to get to the next tile
					//or the AI should do something else
					timeOut += Time.deltaTime;
					if (timeOut > 5f)
					{
						ResetMovingValues();
						Logger.Log("Path following timed out. Something must be in the way", Category.Movement);
						FollowCompleted();
						yield break;
					}
				}
			}

			yield return WaitFor.EndOfFrame;
		}

		yield return WaitFor.EndOfFrame;

		ResetMovingValues();
		FollowCompleted();
	}

	private void ResetMovingValues()
	{
		movingToTile = false;
		arrivedAtTile = false;
		timeOut = 0f;
		status = Status.idle;
	}

	/// <summary>
	/// This is called whenever a follow path has been completed
	/// It is also called on a path follow timeout or if something has
	/// blocked the path and the NPC cannot go any further
	/// </summary>
	protected virtual void FollowCompleted() { }

	/// <summary>
	/// This method is called if something has moved into the path
	/// and has been blocking the agents path forward for
	/// more then 5 seconds. PathFinder will go back to idle when
	/// this is called
	/// </summary>
	protected virtual void PathMoveTimedOut() { }

	protected virtual void OnTileReached(Vector3Int tilePos)
	{
		if (movingToTile && !arrivedAtTile)
		{
			arrivedAtTile = true;
		}
	}

	private void RefreshNodeType(Node node)
	{
		Vector3Int checkPos = new Vector3Int(node.position.x,
			node.position.y, 0);

		if (matrix.IsPassableAt(checkPos, true))
		{
			node.nodeType = PathFinding.NodeType.Open;
			return;
		}
		else
		{
			var getDoor = matrix.GetFirst<DoorController>(checkPos, true);
			if (getDoor)
			{
				if ((int) getDoor.AccessRestrictions.restriction == 0)
				{
					node.nodeType = PathFinding.NodeType.Open;
				}
				else
				{
					node.nodeType = PathFinding.NodeType.Blocked;
				}
			}
			else
			{
				node.nodeType = PathFinding.NodeType.Blocked;
			}
		}
	}

	private static float GetNodeDistance(Node source, Node target)
	{
		int dx = Mathf.Abs(source.position.x - target.position.x);
		int dy = Mathf.Abs(source.position.y - target.position.y);

		int min = Mathf.Min(dx, dy);
		int max = Mathf.Max(dx, dy);

		int diagonalSteps = min;
		int straightSteps = max - min;

		return (1.4f * diagonalSteps + straightSteps);
	}
}