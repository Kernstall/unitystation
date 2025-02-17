internal enum MessageTypes : short
{
	//Server messages - 1xxx
	GibMessage = 1000,
	RunMethodMessage = 1001,
	UpdateChatMessage = 1002,
	UpdateConnectedPlayersMessage = 1003,
	UpdateRoundTimeMessage = 1004,
	UpdateSlotMessage = 1005,
	UpdateUIMessage = 1006,
	ClosetHandlerMessage = 1007,
	ForceJobListUpdateMessage = 1008,
	TransformStateMessage = 1009,
	PlayerDeathMessage = 1010,
	InfoWindowMessage = 1011,
	PlayerMoveMessage = 1012,
	MatrixMoveMessage = 1013,
	ShootMessage = 1014,
	TabUpdateMessage = 1015,
	DoorUpdateMessage = 1016,
	AnnouncementMessage = 1017,
	TileChangesNewClientSync = 1018,
	PlaySoundMessage = 1019,
	ProgressBarMessage = 1020,
	PaperUpdateMessage = 1021,
	StorageObjectSyncMessage = 1022,
	// = 1023, unused, replace at will
	// = 1024, unused, replace at will
	ElectricalStatsMessage = 1025,
	InformPull = 1026,
	HealthOverallStats = 1027,
	HealthConsciousState = 1028,
	HealthRespiratoryStats = 1029,
	HealthBloodStats = 1030,
	HealthBrainStats = 1031,
	HealthBodyPartStats = 1032,
	TriggerEvent = 1033,
	Suicide = 1034,
	PlayerUprightMessage = 1035,
	ElectricalCableMessage = 1036,
	HealthTemperatureStats = 1037,
	HealthPressureStats = 1038,
	EquipmentSpritesMessage = 1039,
	PlayerCustomisationMessage = 1040,
	BookNetMessage = 1041,
	BookshelfNetMessage = 1042,
	SubBookshelfNetMessage = 1043,
	UpdateCountdownMessage = 1044,

	//Client messages - 2xxx
	UpdateHeadsetKeyMessage = 2000,
	InteractMessage = 2001,
	StoreItemMessage = 2002,
	PostToChatMessage = 2003,
	OpenPageValueNetMessage = 2004,
	SimpleInteractMessage = 2005,
	RequestSyncMessage = 2006,
	RequestAuthMessage = 2007,
	RequestMoveMessage = 2008,
	RequestShootMessage = 2009,
	TabInteractMessage = 2010,
	UIInteractMessage = 2011,
	RequestTileDeconstructMessage = 2012,
	RequestElectricalStats = 2013,
	RequestHealthStats = 2014,
	DevSpawnMessage = 2015,
	DevCloneMessage = 2016,
	DevDestroyMessage = 2017,
	RequestMouseDropMessage = 2018,
	RequestHandApplyMessage = 2019,
	RequestAimApplyMessage = 2020,
	RequestHandActivateMessage = 2021,
	// = 2022, unused, replace at will
	RequestPositionalHandApplyMessage = 2023,
	RequestActivateMessage = 2024,
	RequestToViewObjectsAtTile = 2026,
	RequestBookshelfNetMessage = 2027,
	RequestOpenBookIDNetMessage = 2028,
	RequestUncuffMessage = 2029,
}