﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Atmospherics
{
	/// <summary>
	/// Represents a mix of gases
	/// </summary>
	public struct GasMix
	{
		public readonly float[] Gases;

		public float Pressure { get; internal set; } // in kPA
		public float Volume { get; private set; } // in m3

		public float Moles => Gases.Sum();

		public float Temperature
		{
			get => AtmosUtils.CalcTemperature(Pressure, Volume, Moles);
			set => Pressure = AtmosUtils.CalcPressure(Volume, Moles, value);
		}

		public float TemperatureCache { get; private set; }

		public float HeatCapacity
		{
			get
			{
				float capacity = 0f;
				foreach (Gas gas in Gas.All)
				{
					capacity += gas.SpecificHeat * Gases[gas];
				}

				return capacity;
			}
		}

		private GasMix(float[] gases, float pressure, float volume = AtmosConstants.TileVolume)
		{
			Gases = gases;
			Pressure = pressure;
			Volume = volume;
			TemperatureCache = 0f;
		}

		public GasMix(GasMix other)
		{
			this = FromPressure((float[])other.Gases.Clone(), other.Pressure, other.Volume);
		}

		public static GasMix FromTemperature(float[] gases, float temperature, float volume = AtmosConstants.TileVolume)
		{
			float pressure = 0;

			if (temperature >= 0)
			{
				pressure = AtmosUtils.CalcPressure(volume, gases.Sum(), temperature);
			}

			return FromPressure(gases, pressure, volume);
		}

		public static GasMix FromPressure(IEnumerable<float> gases, float pressure, float volume = AtmosConstants.TileVolume)
		{
			return new GasMix(gases.ToArray(), pressure, volume);
		}

		public static GasMix operator +(GasMix a, GasMix b)
		{
			float[] gases = new float[Gas.Count];

			for (int i = 0; i < Gas.Count; i++)
			{
				gases[i] = a.Gases[i] + b.Gases[i];
			}

			float pressure = a.Pressure + b.Pressure * b.Volume / a.Volume;

			return new GasMix(gases, pressure, a.Volume);
		}

		public static GasMix operator -(GasMix a, GasMix b)
		{
			float[] gases = new float[Gas.Count];

			for (int i = 0; i < Gas.Count; i++)
			{
				gases[i] = a.Gases[i] - b.Gases[i];
			}

			float pressure = a.Pressure - b.Pressure * b.Volume / a.Volume;

			return new GasMix(gases, pressure, a.Volume);
		}

		public static GasMix operator *(GasMix a, float factor)
		{
			float[] gases = new float[Gas.Count];

			for (int i = 0; i < Gas.Count; i++)
			{
				gases[i] = a.Gases[i] * factor;
			}

			float pressure = a.Pressure * factor;

			return new GasMix(gases, pressure, a.Volume);
		}

		public static GasMix operator /(GasMix a, float factor)
		{
			float[] gases = new float[Gas.Count];

			for (int i = 0; i < Gas.Count; i++)
			{
				gases[i] = a.Gases[i] / factor;
			}

			float pressure = a.Pressure / factor;

			return new GasMix(gases, pressure, a.Volume);
		}

		public float GetPressure(Gas gas)
		{
			return Moles > 0 ? Pressure * Gases[gas] / Moles : 0;
		}

		public float GetMoles(Gas gas)
		{
			return Gases[gas];
		}

		public void ChangeVolumeValue(float value)
		{
			Volume += value;
			Recalculate();
		}

		public GasMix RemoveVolume(float volume, bool setVolume = false)
		{
			GasMix removed = RemoveRatio(volume / Volume);

			if (setVolume)
			{
				removed.Volume = volume;
				removed = FromTemperature(removed.Gases, Temperature, volume);
			}

			return removed;
		}

		public GasMix RemoveRatio(float ratio)
		{
			GasMix removed = this * ratio;

			for (int i = 0; i < Gas.Count; i++)
			{
				Gases[i] -= removed.Gases[i];
			}

			Pressure -= removed.Pressure * removed.Volume / Volume;

			return removed;
		}

		public void MergeGasMix(GasMix otherGas)
		{
			float totalVolume = Volume + otherGas.Volume;
			for (int i = 0; i < Gas.Count; i++)
			{
				float gas = (Gases[i] + otherGas.Gases[i]) / totalVolume;
				Gases[i] = gas * Volume;
				otherGas.Gases[i] = gas * otherGas.Volume;
			}
			Recalculate();
			otherGas.Recalculate();
		}

		public void AddGas(Gas gas, float moles)
		{
			TemperatureCache = Temperature;
			Gases[gas] += moles;

			RecalculateTemperatureCache();
		}

		public void RemoveGas(Gas gas, float moles)
		{
			TemperatureCache = Temperature;
			Gases[gas] -= moles;
			RecalculateTemperatureCache();
		}

		public GasMix RemoveGasReturn(Gas gas, float moles)
		{
			TemperatureCache = Temperature;
			Gases[gas] -= moles;
			RecalculateTemperatureCache();
			return (this);
		}

		public void Copy(GasMix other)
		{
			for (int i = 0; i < Gas.Count; i++)
			{
				Gases[i] = other.Gases[i];
			}

			Pressure = other.Pressure;
			Volume = other.Volume;
		}

		public override string ToString()
		{
			return $"{Pressure} kPA, {Temperature} K, {Moles} mol, {Volume * 1000} L";
		}

		private void Recalculate()
		{
			Pressure = AtmosUtils.CalcPressure(Volume, Moles, Temperature);
		}


		//Used to change the pressure instead of temperature when removing/Adding gas
		private void RecalculateTemperatureCache()
		{
			Pressure = AtmosUtils.CalcPressure(Volume, Moles, TemperatureCache);
		}
	}
}