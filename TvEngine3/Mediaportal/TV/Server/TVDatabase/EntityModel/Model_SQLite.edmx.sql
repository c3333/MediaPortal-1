-- ------------------------------------------------------------------------------
-- This is auto-generated SQL script.
-- ------------------------------------------------------------------------------
-- This DDL was generated by 'Devart SSDLToSQLite.tt' template.
-- SQL is generated on: 12/02/2013 19:26:13
-- Generated from EDMX file: D:\svnroot\MediaPortal-1\TvEngine3\Mediaportal\TV\Server\TVDatabase\EntityModel\Model.edmx
-- ------------------------------------------------------------------------------

-- Note: This file contains customizations and workarounds for EF limitations!

PRAGMA journal_mode=WAL;

-- ------------------------------------------------------------------------------
-- Dropping existing tables
-- ------------------------------------------------------------------------------
DROP TABLE IF EXISTS "CanceledSchedules";
DROP TABLE IF EXISTS "Tuners";
DROP TABLE IF EXISTS "TunerGroups";
DROP TABLE IF EXISTS "Channels";
DROP TABLE IF EXISTS "ChannelGroups";
DROP TABLE IF EXISTS "ChannelLinkageMaps";
DROP TABLE IF EXISTS "ChannelMaps";
DROP TABLE IF EXISTS "Conflicts";
DROP TABLE IF EXISTS "DiseqcMotors";
DROP TABLE IF EXISTS "GroupMaps";
DROP TABLE IF EXISTS "Histories";
DROP TABLE IF EXISTS "PendingDeletions";
DROP TABLE IF EXISTS "Programs";
DROP TABLE IF EXISTS "ProgramCategories";
DROP TABLE IF EXISTS "ProgramCredits";
DROP TABLE IF EXISTS "Recordings";
DROP TABLE IF EXISTS "RuleBasedSchedules";
DROP TABLE IF EXISTS "Satellites";
DROP TABLE IF EXISTS "Schedules";
DROP TABLE IF EXISTS "ScheduleRulesTemplates";
DROP TABLE IF EXISTS "Settings";
DROP TABLE IF EXISTS "SoftwareEncoders";
DROP TABLE IF EXISTS "TuningDetails";
DROP TABLE IF EXISTS "Versions";
DROP TABLE IF EXISTS "LnbTypes";
DROP TABLE IF EXISTS "RecordingCredits";
DROP TABLE IF EXISTS "GuideCategories";
DROP TABLE IF EXISTS "TunerProperties";
DROP TABLE IF EXISTS "AnalogTunerSettings";

-- ------------------------------------------------------------------------------
-- Creating all tables
-- ------------------------------------------------------------------------------

-- Table "CanceledSchedules"
CREATE TABLE "CanceledSchedules"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdCanceledSchedule" int UNIQUE,
    "IdSchedule" int NOT NULL,
    "IdChannel" int NOT NULL,
    "CancelDateTime" datetime NOT NULL,
    CONSTRAINT "FK_ScheduleCanceledSchedule" FOREIGN KEY ("IdSchedule")
    REFERENCES "Schedules" ("IdSchedule") ON DELETE CASCADE
);

CREATE INDEX Idx_CanceledSchedules_1 ON  "CanceledSchedules" (IdSchedule);

CREATE TRIGGER "CanceledSchedules_autoincrement" AFTER INSERT ON "CanceledSchedules"
  FOR EACH ROW BEGIN
    UPDATE CanceledSchedules SET IdCanceledSchedule = Id WHERE Id = NEW.Id; 
  END;

-- Table "Tuners"
CREATE TABLE "Tuners"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdTuner" int UNIQUE,
    "ExternalId" varchar(200) NOT NULL COLLATE NOCASE,
    "Name" varchar(200) NOT NULL COLLATE NOCASE,
    "Priority" int NOT NULL,
    "IsEnabled" bit NOT NULL,
    "UseForEpgGrabbing" bit NOT NULL,
    "RecordingFolder" varchar(300) NOT NULL COLLATE NOCASE,
    "TimeshiftingFolder" varchar(300) NOT NULL COLLATE NOCASE,
    "SupportedBroadcastStandards" int NOT NULL,
    "UseConditionalAccess" bit NOT NULL,
    "CamType" int NOT NULL,
    "DecryptLimit" int NOT NULL,
    "MultiChannelDecryptMode" int NOT NULL,
    "ConditionalAccessProviders" varchar(1000) NOT NULL COLLATE NOCASE,
    "Preload" bit NOT NULL,
    "BdaNetworkProvider" int NOT NULL,
    "IdleMode" int NOT NULL,
    "AlwaysSendDiseqcCommands" bit NOT NULL,
    "PidFilterMode" int NOT NULL,
    "UseCustomTuning" bit NOT NULL,
    "IdTunerGroup" int NULL,
    "DumpTsWriterInputs" int NOT NULL,
    "DumpTsMuxerInputs" int NOT NULL,
    "DisableTsWriterCrcChecking" bit NOT NULL,
    CONSTRAINT "FK_TunerGroupTuner" FOREIGN KEY ("IdTunerGroup")
    REFERENCES "TunerGroups" ("IdTunerGroup")
);

CREATE INDEX Idx_Tuners_1 ON  "Tuners" (IdTunerGroup);

CREATE TRIGGER "Tuners_autoincrement" AFTER INSERT ON "Tuners"
  FOR EACH ROW BEGIN
    UPDATE Tuners SET IdTuner = Id WHERE Id = NEW.Id; 
  END;

-- Table "TunerGroups"
CREATE TABLE "TunerGroups"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdTunerGroup" int UNIQUE,
    "Name" varchar(200) NOT NULL COLLATE NOCASE
);

CREATE TRIGGER "TunerGroups_autoincrement" AFTER INSERT ON "TunerGroups"
  FOR EACH ROW BEGIN
    UPDATE TunerGroups SET IdTunerGroup = Id WHERE Id = NEW.Id; 
  END;

-- Table "Channels"
CREATE TABLE "Channels"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdChannel" int UNIQUE,
    "TimesWatched" int NOT NULL,
    "TotalTimeWatched" datetime NULL,
    "LastGrabTime" datetime NULL,
    "VisibleInGuide" bit NOT NULL,
    "ExternalId" varchar(200) NULL COLLATE NOCASE,
    "Name" varchar(200) NOT NULL COLLATE NOCASE,
    "MediaType" int NOT NULL,
    "ChannelNumber" varchar(10) NOT NULL COLLATE NOCASE
);

CREATE TRIGGER "Channels_autoincrement" AFTER INSERT ON "Channels"
  FOR EACH ROW BEGIN
    UPDATE Channels SET IdChannel = Id WHERE Id = NEW.Id; 
  END;

-- Table "ChannelGroups"
CREATE TABLE "ChannelGroups"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdGroup" int UNIQUE,
    "GroupName" varchar(200) NOT NULL COLLATE NOCASE,
    "SortOrder" int NOT NULL,
    "MediaType" int NOT NULL
);

CREATE TRIGGER "ChannelGroups_autoincrement" AFTER INSERT ON "ChannelGroups"
  FOR EACH ROW BEGIN
    UPDATE ChannelGroups SET IdGroup = Id WHERE Id = NEW.Id; 
  END;

-- Table "ChannelLinkageMaps"
CREATE TABLE "ChannelLinkageMaps"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdMapping" int UNIQUE,
    "IdPortalChannel" int NOT NULL,
    "IdLinkedChannel" int NOT NULL,
    "Name" varchar(200) NOT NULL COLLATE NOCASE,
    CONSTRAINT "FK_ChannelLinkMap" FOREIGN KEY ("IdLinkedChannel")
    REFERENCES "Channels" ("IdChannel") ON DELETE CASCADE,
    CONSTRAINT "FK_ChannelPortalMap" FOREIGN KEY ("IdPortalChannel")
    REFERENCES "Channels" ("IdChannel")
);

CREATE INDEX Idx_ChannelLinkageMaps_1 ON  "ChannelLinkageMaps" (IdLinkedChannel);
CREATE INDEX Idx_ChannelLinkageMaps_2 ON  "ChannelLinkageMaps" (IdPortalChannel);

CREATE TRIGGER "ChannelLinkageMaps_autoincrement" AFTER INSERT ON "ChannelLinkageMaps"
  FOR EACH ROW BEGIN
    UPDATE ChannelLinkageMaps SET IdMapping = Id WHERE Id = NEW.Id; 
  END;

-- Table "ChannelMaps"
CREATE TABLE "ChannelMaps"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdChannelMap" int UNIQUE,
    "IdChannel" int NOT NULL,
    "IdTuner" int NOT NULL,
    CONSTRAINT "FK_TunerChannelMap" FOREIGN KEY ("IdTuner")
    REFERENCES "Tuners" ("IdTuner") ON DELETE CASCADE,
    CONSTRAINT "FK_ChannelChannelMap" FOREIGN KEY ("IdChannel")
    REFERENCES "Channels" ("IdChannel") ON DELETE CASCADE
);

CREATE INDEX Idx_ChannelMaps_1 ON  "ChannelMaps" (IdChannel);
CREATE INDEX Idx_ChannelMaps_2 ON  "ChannelMaps" (IdTuner);

CREATE TRIGGER "ChannelMaps_autoincrement" AFTER INSERT ON "ChannelMaps"
  FOR EACH ROW BEGIN
    UPDATE ChannelMaps SET IdChannelMap = Id WHERE Id = NEW.Id; 
  END;

-- Table "Conflicts"
CREATE TABLE "Conflicts"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdConflict" int UNIQUE,
    "IdSchedule" int NOT NULL,
    "IdConflictingSchedule" int NOT NULL,
    "IdChannel" int NOT NULL,
    "ConflictDate" datetime NOT NULL,
    "IdTuner" int NULL,
    CONSTRAINT "FK_TunerConflict" FOREIGN KEY ("IdTuner")
    REFERENCES "Tuners" ("IdTuner") ON DELETE CASCADE,
    CONSTRAINT "FK_ChannelConflict" FOREIGN KEY ("IdChannel")
    REFERENCES "Channels" ("IdChannel") ON DELETE CASCADE,
    CONSTRAINT "FK_ScheduleConflict" FOREIGN KEY ("IdSchedule")
    REFERENCES "Schedules" ("IdSchedule"),
    CONSTRAINT "FK_ScheduleConflict1" FOREIGN KEY ("IdConflictingSchedule")
    REFERENCES "Schedules" ("IdSchedule")
);

CREATE INDEX Idx_Conflicts_1 ON  "Conflicts" (IdTuner);
CREATE INDEX Idx_Conflicts_2 ON  "Conflicts" (IdChannel);
CREATE INDEX Idx_Conflicts_3 ON  "Conflicts" (IdSchedule);
CREATE INDEX Idx_Conflicts_4 ON  "Conflicts" (IdConflictingSchedule);

CREATE TRIGGER "Conflicts_autoincrement" AFTER INSERT ON "Conflicts"
  FOR EACH ROW BEGIN
    UPDATE Conflicts SET IdConflict = Id WHERE Id = NEW.Id; 
  END;

-- Table "DiseqcMotors"
CREATE TABLE "DiseqcMotors"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdDiseqcMotor" int UNIQUE,
    "IdTuner" int NOT NULL,
    "IdSatellite" int NOT NULL,
    "Position" int NOT NULL,
    CONSTRAINT "FK_DiseqcMotorTuner" FOREIGN KEY ("IdTuner")
    REFERENCES "Tuners" ("IdTuner") ON DELETE CASCADE,
    CONSTRAINT "FK_DiseqcMotorSatellite" FOREIGN KEY ("IdSatellite")
    REFERENCES "Satellites" ("IdSatellite") ON DELETE CASCADE
);

CREATE INDEX Idx_DiseqcMotors_1 ON  "DiseqcMotors" (IdTuner);
CREATE INDEX Idx_DiseqcMotors_2 ON  "DiseqcMotors" (IdSatellite);

CREATE TRIGGER "DiseqcMotors_autoincrement" AFTER INSERT ON "DiseqcMotors"
  FOR EACH ROW BEGIN
    UPDATE DiseqcMotors SET IdDiseqcMotor = Id WHERE Id = NEW.Id; 
  END;

-- Table "GroupMaps"
CREATE TABLE "GroupMaps"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdMap" int UNIQUE,
    "IdGroup" int NOT NULL,
    "IdChannel" int NOT NULL,
    "SortOrder" int NOT NULL,
    CONSTRAINT "FK_GroupMapChannelGroup" FOREIGN KEY ("IdGroup")
    REFERENCES "ChannelGroups" ("IdGroup") ON DELETE CASCADE,
    CONSTRAINT "FK_GroupMapChannel" FOREIGN KEY ("IdChannel")
    REFERENCES "Channels" ("IdChannel") ON DELETE CASCADE
);

CREATE INDEX Idx_GroupMaps_1 ON  "GroupMaps" (IdGroup);
CREATE INDEX Idx_GroupMaps_2 ON  "GroupMaps" (IdChannel);

CREATE TRIGGER "GroupMaps_autoincrement" AFTER INSERT ON "GroupMaps"
  FOR EACH ROW BEGIN
    UPDATE GroupMaps SET IdMap = Id WHERE Id = NEW.Id; 
  END;

-- Table "Histories"
CREATE TABLE "Histories"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdHistory" int UNIQUE,
    "IdChannel" int NOT NULL,
    "StartTime" datetime NOT NULL,
    "EndTime" datetime NOT NULL,
    "Title" varchar(1000) NOT NULL COLLATE NOCASE,
    "Description" varchar(1000) NOT NULL COLLATE NOCASE,
    "Recorded" bit NOT NULL,
    "Watched" int NOT NULL,
    "IdProgramCategory" int NULL,
    CONSTRAINT "FK_ChannelHistory" FOREIGN KEY ("IdChannel")
    REFERENCES "Channels" ("IdChannel") ON DELETE CASCADE,
    CONSTRAINT "FK_ProgramCategoryHistory" FOREIGN KEY ("IdProgramCategory")
    REFERENCES "ProgramCategories" ("IdProgramCategory")
);

CREATE INDEX Idx_Histories_1 ON  "Histories" (IdChannel);
CREATE INDEX Idx_Histories_2 ON  "Histories" (IdProgramCategory);

CREATE TRIGGER "Histories_autoincrement" AFTER INSERT ON "Histories"
  FOR EACH ROW BEGIN
    UPDATE Histories SET IdHistory = Id WHERE Id = NEW.Id; 
  END;

-- Table "PendingDeletions"
CREATE TABLE "PendingDeletions"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdPendingDeletion" int UNIQUE,
    "FileName" text NOT NULL
);

CREATE TRIGGER "PendingDeletions_autoincrement" AFTER INSERT ON "PendingDeletions"
  FOR EACH ROW BEGIN
    UPDATE PendingDeletions SET IdPendingDeletion = Id WHERE Id = NEW.Id; 
  END;

-- Table "Programs"
CREATE TABLE "Programs"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdProgram" int UNIQUE,
    "IdChannel" int NOT NULL,
    "StartTime" datetime NOT NULL,
    "EndTime" datetime NOT NULL,
    "Title" varchar(2000) NOT NULL COLLATE NOCASE,
    "Description" varchar(8000) NOT NULL COLLATE NOCASE,
    "EpisodeName" varchar(2000) NULL COLLATE NOCASE,
    "SeriesId" varchar(200) NULL COLLATE NOCASE,
    "SeasonNumber" int NULL,
    "EpisodeId" varchar(200) NULL COLLATE NOCASE,
    "EpisodeNumber" int NULL,
    "EpisodePartNumber" int NULL,
    "IsPreviouslyShown" bit NULL,
    "OriginalAirDate" datetime NULL,
    "IdProgramCategory" int NULL,
    "Classification" varchar(200) NULL COLLATE NOCASE,
    "Advisories" int NOT NULL,
    "IsHighDefinition" bit NULL,
    "IsThreeDimensional" bit NULL,
    "AudioLanguages" varchar(50) NULL COLLATE NOCASE,
    "SubtitlesLanguages" varchar(50) NULL COLLATE NOCASE,
    "IsLive" bit NULL,
    "ProductionYear" int NULL,
    "ProductionCountry" varchar(200) NULL COLLATE NOCASE,
    "StarRating" decimal(10, 2) NULL,
    "StarRatingMaximum" decimal(10, 2) NULL,
    "State" int NOT NULL,
    "StartTimeDayOfWeek" smallint NOT NULL,
    "EndTimeDayOfWeek" smallint NOT NULL,
    "EndTimeOffset" datetime NOT NULL,
    "StartTimeOffset" datetime NOT NULL,
    CONSTRAINT "FK_ChannelProgram" FOREIGN KEY ("IdChannel")
    REFERENCES "Channels" ("IdChannel") ON DELETE CASCADE,
    CONSTRAINT "FK_ProgramProgramCategory" FOREIGN KEY ("IdProgramCategory")
    REFERENCES "ProgramCategories" ("IdProgramCategory")
);

CREATE INDEX Idx_Programs_1 ON  "Programs" (IdChannel);
CREATE INDEX Idx_Programs_2 ON  "Programs" (IdProgramCategory);

CREATE TRIGGER "Programs_autoincrement" AFTER INSERT ON "Programs"
  FOR EACH ROW BEGIN
    UPDATE Programs SET IdProgram = Id WHERE Id = NEW.Id; 
  END;

-- Table "ProgramCategories"
CREATE TABLE "ProgramCategories"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdProgramCategory" int UNIQUE,
    "Category" varchar(50) NOT NULL COLLATE NOCASE,
    "IdGuideCategory" int NULL,
    CONSTRAINT "FK_GuideCategoryProgramCategory" FOREIGN KEY ("IdGuideCategory")
    REFERENCES "GuideCategories" ("IdGuideCategory")
);

CREATE INDEX Idx_ProgramCategories_1 ON "ProgramCategories" (IdGuideCategory);

CREATE TRIGGER "ProgramCategories_autoincrement" AFTER INSERT ON "ProgramCategories"
  FOR EACH ROW BEGIN
    UPDATE ProgramCategories SET IdProgramCategory = Id WHERE Id = NEW.Id; 
  END;

-- Table "ProgramCredits"
CREATE TABLE "ProgramCredits"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdProgramCredit" int UNIQUE,
    "IdProgram" int NOT NULL,
    "Person" varchar(200) NOT NULL COLLATE NOCASE,
    "Role" varchar(50) NOT NULL COLLATE NOCASE,
    CONSTRAINT "FK_ProgramProgramCredit" FOREIGN KEY ("IdProgram")
    REFERENCES "Programs" ("IdProgram") ON DELETE CASCADE
);

CREATE INDEX Idx_ProgramCredits_1 ON "ProgramCredits" (IdProgram);

CREATE TRIGGER "ProgramCredits_autoincrement" AFTER INSERT ON "ProgramCredits"
  FOR EACH ROW BEGIN
    UPDATE ProgramCredits SET IdProgramCredit = Id WHERE Id = NEW.Id; 
  END;

-- Table "Recordings"
CREATE TABLE "Recordings"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdRecording" int UNIQUE,
    "IdChannel" int NULL,
    "StartTime" datetime NOT NULL,
    "EndTime" datetime NOT NULL,
    "Title" varchar(2000) NOT NULL COLLATE NOCASE,
    "Description" varchar(8000) NOT NULL COLLATE NOCASE,
    "FileName" varchar(260) NOT NULL COLLATE NOCASE,
    "KeepUntil" int NOT NULL,
    "KeepUntilDate" datetime NULL,
    "TimesWatched" int NOT NULL,
    "StopTime" int NOT NULL,
    "EpisodeName" text NOT NULL,
    "SeriesNum" varchar(200) NOT NULL COLLATE NOCASE,
    "EpisodeNum" varchar(200) NOT NULL COLLATE NOCASE,
    "EpisodePart" text NOT NULL,
    "IsRecording" bit NOT NULL,
    "IdSchedule" int NULL,
    "MediaType" int NOT NULL,
    "IdProgramCategory" int NULL,
    CONSTRAINT "FK_ChannelRecording" FOREIGN KEY ("IdChannel")
    REFERENCES "Channels" ("IdChannel"),
    CONSTRAINT "FK_ScheduleRecording" FOREIGN KEY ("IdSchedule")
    REFERENCES "Schedules" ("IdSchedule"),
    CONSTRAINT "FK_RecordingProgramCategory" FOREIGN KEY ("IdProgramCategory")
    REFERENCES "ProgramCategories" ("IdProgramCategory")
);

CREATE INDEX Idx_Recordings_1 ON "Recordings" (IdChannel);
CREATE INDEX Idx_Recordings_2 ON "Recordings" (IdSchedule);
CREATE INDEX Idx_Recordings_3 ON "Recordings" (IdProgramCategory);

CREATE TRIGGER "Recordings_autoincrement" AFTER INSERT ON "Recordings"
  FOR EACH ROW BEGIN
    UPDATE Recordings SET IdRecording = Id WHERE Id = NEW.Id; 
  END;

-- Table "RuleBasedSchedules"
CREATE TABLE "RuleBasedSchedules"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdRuleBasedSchedule" int UNIQUE,
    "ScheduleName" varchar(256) NOT NULL COLLATE NOCASE,
    "MaxAirings" int NOT NULL,
    "Priority" int NOT NULL,
    "Directory" varchar(1024) NOT NULL COLLATE NOCASE,
    "Quality" int NOT NULL,
    "KeepMethod" int NOT NULL,
    "KeepDate" datetime NULL,
    "PreRecordInterval" int NOT NULL,
    "PostRecordInterval" int NOT NULL,
    "Rules" text NULL
);

CREATE TRIGGER "RuleBasedSchedules_autoincrement" AFTER INSERT ON "RuleBasedSchedules"
  FOR EACH ROW BEGIN
    UPDATE RuleBasedSchedules SET IdRuleBasedSchedule = Id WHERE Id = NEW.Id; 
  END;

-- Table "Satellites"
CREATE TABLE "Satellites"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdSatellite" int UNIQUE,
    "Longitude" int NOT NULL,
    "Name" varchar(200) NOT NULL COLLATE NOCASE
);

CREATE TRIGGER "Satellites_autoincrement" AFTER INSERT ON "Satellites"
  FOR EACH ROW BEGIN
    UPDATE Satellites SET IdSatellite = Id WHERE Id = NEW.Id; 
  END;

-- Table "Schedules"
CREATE TABLE "Schedules"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdSchedule" int UNIQUE,
    "IdChannel" int NOT NULL,
    "ScheduleType" int NOT NULL,
    "ProgramName" varchar(256) NOT NULL COLLATE NOCASE,
    "StartTime" datetime NOT NULL,
    "EndTime" datetime NOT NULL,
    "MaxAirings" int NOT NULL,
    "Priority" int NOT NULL,
    "Directory" varchar(1024) NOT NULL COLLATE NOCASE,
    "Quality" int NOT NULL,
    "KeepMethod" int NOT NULL,
    "KeepDate" datetime NULL,
    "PreRecordInterval" int,
    "PostRecordInterval" int,
    "Canceled" datetime NOT NULL,
    "Series" bit NOT NULL,
    "IdParentSchedule" int NULL,
    CONSTRAINT "FK_ChannelSchedule" FOREIGN KEY ("IdChannel")
    REFERENCES "Channels" ("IdChannel") ON DELETE CASCADE,
    CONSTRAINT "FK_ScheduleParentSchedule" FOREIGN KEY ("IdParentSchedule")
    REFERENCES "Schedules" ("IdSchedule")
);

CREATE INDEX Idx_Schedules_1 ON "Schedules" (IdChannel);
CREATE INDEX Idx_Schedules_2 ON "Schedules" (IdParentSchedule);

CREATE TRIGGER "Schedules_autoincrement" AFTER INSERT ON "Schedules"
  FOR EACH ROW BEGIN
    UPDATE Schedules SET IdSchedule = Id WHERE Id = NEW.Id; 
  END;

-- Table "ScheduleRulesTemplates"
CREATE TABLE "ScheduleRulesTemplates"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdScheduleRulesTemplate" int UNIQUE,
    "Name" varchar(50) NOT NULL COLLATE NOCASE,
    "Rules" text NOT NULL,
    "Enabled" bit NOT NULL,
    "Usages" int NOT NULL,
    "Editable" bit NOT NULL
);

CREATE TRIGGER "ScheduleRulesTemplates_autoincrement" AFTER INSERT ON "ScheduleRulesTemplates"
  FOR EACH ROW BEGIN
    UPDATE ScheduleRulesTemplates SET IdScheduleRulesTemplate = Id WHERE Id = NEW.Id; 
  END;

-- Table "Settings"
CREATE TABLE "Settings"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdSetting" int UNIQUE,
    "Tag" varchar(200) NOT NULL COLLATE NOCASE,
    "Value" varchar(4096) NOT NULL COLLATE NOCASE
);

CREATE TRIGGER "Settings_autoincrement" AFTER INSERT ON "Settings"
  FOR EACH ROW BEGIN
    UPDATE Settings SET IdSetting = Id WHERE Id = NEW.Id; 
  END;

-- Table "SoftwareEncoders"
CREATE TABLE "SoftwareEncoders"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdSoftwareEncoder" int UNIQUE,
    "Priority" int NOT NULL,
    "Name" varchar(200) NOT NULL COLLATE NOCASE,
    "Type" int NOT NULL,
    "ClassId" varchar(50) NOT NULL COLLATE NOCASE
);

CREATE TRIGGER "SoftwareEncoders_autoincrement" AFTER INSERT ON "SoftwareEncoders"
  FOR EACH ROW BEGIN
    UPDATE SoftwareEncoders SET IdSoftwareEncoder = Id WHERE Id = NEW.Id; 
  END;

-- Table "TuningDetails"
CREATE TABLE "TuningDetails"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdTuning" int UNIQUE,
    "IdChannel" int NOT NULL,
    "MediaType" int NOT NULL,
    "Priority" int NOT NULL,
    "BroadcastStandard" int NOT NULL,
    "Name" varchar(200) NOT NULL COLLATE NOCASE,
    "Provider" varchar(200) NOT NULL COLLATE NOCASE,
    "LogicalChannelNumber" varchar(10) NOT NULL COLLATE NOCASE,
    "IsEncrypted" bit NOT NULL,
    "IsHighDefinition" bit NOT NULL,
    "IsThreeDimensional" bit NOT NULL,
    "OriginalNetworkId" int NOT NULL,
    "TransportStreamId" int NOT NULL,
    "ServiceId" int NOT NULL,
    "FreesatChannelId" int NOT NULL,
    "OpenTvChannelId" int NOT NULL,
    "EpgOriginalNetworkId" int NOT NULL,
    "EpgTransportStreamId" int NOT NULL,
    "EpgServiceId" int NOT NULL,
    "SourceId" int NOT NULL,
    "PmtPid" int NOT NULL,
    "PhysicalChannelNumber" int NOT NULL,
    "Frequency" int NOT NULL,
    "CountryId" int NOT NULL,
    "Modulation" int NOT NULL,
    "Polarisation" int NOT NULL,
    "SymbolRate" int NOT NULL,
    "DiSEqC" int NOT NULL,
    "Bandwidth" int NOT NULL,
    "VideoSource" int NOT NULL,
    "TuningSource" int NOT NULL,
    "SatIndex" int NOT NULL,
    "FecCodeRate" int NOT NULL,
    "PilotTonesState" int NOT NULL,
    "RollOffFactor" int NOT NULL,
    "StreamId" int NOT NULL,
    "Url" varchar(200) NOT NULL COLLATE NOCASE,
    "AudioSource" int NOT NULL,
    "IsVcrSignal" bit NOT NULL,
    "IdLnbType" int NULL,
    CONSTRAINT "FK_ChannelTuningDetail" FOREIGN KEY ("IdChannel")
    REFERENCES "Channels" ("IdChannel") ON DELETE CASCADE,
    CONSTRAINT "FK_LnbTypeTuningDetail" FOREIGN KEY ("IdLnbType")
    REFERENCES "LnbTypes" ("IdLnbType")
);

CREATE INDEX Idx_TuningDetails_1 ON "TuningDetails" (IdChannel);
CREATE INDEX Idx_TuningDetails_2 ON "TuningDetails" (IdLnbType);

CREATE TRIGGER "TuningDetails_autoincrement" AFTER INSERT ON "TuningDetails"
  FOR EACH ROW BEGIN
    UPDATE TuningDetails SET IdTuning = Id WHERE Id = NEW.Id; 
  END;

-- Table "Versions"
CREATE TABLE "Versions"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdVersion" int UNIQUE,
    "VersionNumber" int NOT NULL
);

CREATE TRIGGER "Versions_autoincrement" AFTER INSERT ON "Versions"
  FOR EACH ROW BEGIN
    UPDATE Versions SET IdVersion = Id WHERE Id = NEW.Id; 
  END;

-- Table "LnbTypes"
CREATE TABLE "LnbTypes"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdLnbType" int UNIQUE,
    "Name" text NOT NULL,
    "LowBandFrequency" int NOT NULL,
    "HighBandFrequency" int NOT NULL,
    "SwitchFrequency" int NOT NULL,
    "IsBandStacked" bit NOT NULL
);

CREATE TRIGGER "LnbTypes_autoincrement" AFTER INSERT ON "LnbTypes"
  FOR EACH ROW BEGIN
    UPDATE LnbTypes SET IdLnbType = Id WHERE Id = NEW.Id; 
  END;

-- Table "RecordingCredits"
CREATE TABLE "RecordingCredits"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdRecordingCredit" int UNIQUE,
    "IdRecording" int NOT NULL,
    "Person" varchar(200) NOT NULL COLLATE NOCASE,
    "Role" varchar(50) NOT NULL COLLATE NOCASE,
    CONSTRAINT "FK_RecordingRecordingCredit" FOREIGN KEY ("IdRecording")
    REFERENCES "Recordings" ("IdRecording") ON DELETE CASCADE
);

CREATE INDEX Idx_RecordingCredits_1 ON "RecordingCredits" (IdRecording);

CREATE TRIGGER "RecordingCredits_autoincrement" AFTER INSERT ON "RecordingCredits"
  FOR EACH ROW BEGIN
    UPDATE RecordingCredits SET IdRecordingCredit = Id WHERE Id = NEW.Id; 
  END;

-- Table "GuideCategories"
CREATE TABLE "GuideCategories"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdGuideCategory" int UNIQUE,
    "Name" text NOT NULL,
    "IsMovie" bit NOT NULL,
    "IsEnabled" bit NOT NULL
);

CREATE TRIGGER "GuideCategories_autoincrement" AFTER INSERT ON "GuideCategories"
  FOR EACH ROW BEGIN
    UPDATE GuideCategories SET IdGuideCategory = Id WHERE Id = NEW.Id; 
  END;

-- Table "TunerProperties"
CREATE TABLE "TunerProperties"  ( 
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdTunerProperty" int UNIQUE,
    "PropertyId" int NOT NULL,
    "Value" int NOT NULL,
    "Default" int NOT NULL,
    "Minimum" int NOT NULL,
    "Maximum" int NOT NULL,
    "Step" int NOT NULL,
    "PossibleValueFlags" int NOT NULL,
    "ValueFlags" int NOT NULL,
    "IdTuner" int NOT NULL,
    CONSTRAINT "FK_TunerTunerProperty" FOREIGN KEY ("IdTuner")
    REFERENCES "Tuners" ("IdTuner") ON DELETE CASCADE
);

CREATE INDEX Idx_TunerProperties_1 ON  "TunerProperties" (IdTuner);

CREATE TRIGGER "TunerProperties_autoincrement" AFTER INSERT ON "TunerProperties"
  FOR EACH ROW BEGIN
    UPDATE TunerProperties SET IdTunerProperty = Id WHERE Id = NEW.Id; 
  END;

-- Table "AnalogTunerSettings"
CREATE TABLE "AnalogTunerSettings"(
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "IdAnalogTunerSettings" int UNIQUE,
    "VideoStandard" int NOT NULL,
    "SupportedVideoStandards" int NOT NULL,
    "FrameSize" int NOT NULL,
    "SupportedFrameSizes" int NOT NULL,
    "FrameRate" int NOT NULL,
    "SupportedFrameRates" int NOT NULL,
    "IdSoftwareEncoderVideo" int NOT NULL,
    "IdSoftwareEncoderAudio" int NOT NULL,
    "EncoderBitRateModeTimeShifting" int NOT NULL,
    "EncoderBitRateTimeShifting" int NOT NULL,
    "EncoderBitRatePeakTimeShifting" int NOT NULL,
    "EncoderBitRateModeRecording" int NOT NULL,
    "EncoderBitRateRecording" int NOT NULL,
    "EncoderBitRatePeakRecording" int NOT NULL,
    "ExternalInputSourceVideo" int NOT NULL,
    "ExternalInputSourceAudio" int NOT NULL,
    "ExternalInputCountryId" int NOT NULL,
    "ExternalInputPhysicalChannelNumber" int NOT NULL,
    "ExternalTunerProgram" varchar(300) NOT NULL COLLATE NOCASE,
    "ExternalTunerProgramArguments" varchar(200) NOT NULL COLLATE NOCASE,
    "SupportedVideoSources" int NOT NULL,
    "SupportedAudioSources" int NOT NULL,
    CONSTRAINT "FK_AnalogTunerSettingsSoftwareEncoderVideo" FOREIGN KEY ("IdSoftwareEncoderVideo")
    REFERENCES "SoftwareEncoders" ("IdSoftwareEncoder"),
    CONSTRAINT "FK_AnalogTunerSettingsSoftwareEncoderAudio" FOREIGN KEY ("IdSoftwareEncoderAudio")
    REFERENCES "SoftwareEncoders" ("IdSoftwareEncoder"),
    CONSTRAINT "FK_TunerAnalogTunerSettings" FOREIGN KEY ("IdAnalogTunerSettings")
    REFERENCES "Tuners" ("IdTuner") ON DELETE CASCADE
);

CREATE INDEX Idx_AnalogTunerSettings_1 ON  "AnalogTunerSettings" (IdSoftwareEncoderVideo);
CREATE INDEX Idx_AnalogTunerSettings_2 ON  "AnalogTunerSettings" (IdSoftwareEncoderAudio);

/* mm1352000: IdAnalogTunerSettings is a foreign key to Tuner.IdTuner, so no trigger required.
CREATE TRIGGER "AnalogTunerSettings_autoincrement" AFTER INSERT ON "AnalogTunerSettings"
  FOR EACH ROW BEGIN
    UPDATE AnalogTunerSettings SET IdAnalogTunerSettings = Id WHERE Id = NEW.Id; 
  END;*/
