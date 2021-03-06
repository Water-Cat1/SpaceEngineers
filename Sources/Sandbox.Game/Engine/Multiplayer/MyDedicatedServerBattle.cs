﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Engine.Utils;
using Sandbox.Game.Multiplayer;
using SteamSDK;

namespace Sandbox.Engine.Multiplayer
{
    [ProtoBuf.ProtoContract]
    [MessageId(13873, P2PMessageEnum.Reliable)]
    public struct ServerBattleDataMsg
    {
        [ProtoBuf.ProtoMember]
        public string WorldName;

        [ProtoBuf.ProtoMember]
        public MyGameModeEnum GameMode;

        [ProtoBuf.ProtoMember]
        public string HostName;

        [ProtoBuf.ProtoMember]
        public ulong WorldSize;

        [ProtoBuf.ProtoMember]
        public int AppVersion;

        [ProtoBuf.ProtoMember]
        public int MembersLimit;

        [ProtoBuf.ProtoMember]
        public string DataHash;

        [ProtoBuf.ProtoMember]
        public List<MyMultiplayerBattleData.KeyValueDataMsg> BattleData;
    }

    public class MyDedicatedServerBattle : MyDedicatedServerBase
    {
        private MyMultiplayerBattleData m_battleData;

        public override float InventoryMultiplier
        {
            get;
            set;
        }

        public override float AssemblerMultiplier
        {
            get;
            set;
        }

        public override float RefineryMultiplier
        {
            get;
            set;
        }

        public override float WelderMultiplier
        {
            get;
            set;
        }

        public override float GrinderMultiplier
        {
            get;
            set;
        }

        public override bool Scenario
        {
            get;
            set;
        }

        public override string ScenarioBriefing
        {
            get;
            set;
        }

        public override DateTime ScenarioStartTime
        {
            get;
            set;
        }

        public override bool Battle
        {
            get { return true; }
            set { }
        }

        public override bool BattleCanBeJoined
        {
            get { return m_battleData.BattleCanBeJoined; }
            set { m_battleData.BattleCanBeJoined = value; }
        }

        public override ulong BattleWorldWorkshopId
        {
            get { return m_battleData.BattleWorldWorkshopId; }
            set { m_battleData.BattleWorldWorkshopId = value; }
        }

        public override int BattleFaction1MaxBlueprintPoints
        {
            get { return m_battleData.BattleFaction1MaxBlueprintPoints; }
            set { m_battleData.BattleFaction1MaxBlueprintPoints = value; }
        }

        public override int BattleFaction2MaxBlueprintPoints
        {
            get { return m_battleData.BattleFaction2MaxBlueprintPoints; }
            set { m_battleData.BattleFaction2MaxBlueprintPoints = value; }
        }

        public override int BattleFaction1BlueprintPoints
        {
            get { return m_battleData.BattleFaction1BlueprintPoints; }
            set { m_battleData.BattleFaction1BlueprintPoints = value; }
        }

        public override int BattleFaction2BlueprintPoints
        {
            get { return m_battleData.BattleFaction2BlueprintPoints; }
            set { m_battleData.BattleFaction2BlueprintPoints = value; }
        }

        public override int BattleMapAttackerSlotsCount
        {
            get { return m_battleData.BattleMapAttackerSlotsCount; }
            set { m_battleData.BattleMapAttackerSlotsCount = value; }
        }

        public override long BattleFaction1Id
        {
            get { return m_battleData.BattleFaction1Id; }
            set { m_battleData.BattleFaction1Id = value; }
        }

        public override long BattleFaction2Id
        {
            get { return m_battleData.BattleFaction2Id; }
            set { m_battleData.BattleFaction2Id = value; }
        }

        public override int BattleFaction1Slot
        {
            get { return m_battleData.BattleFaction1Slot; }
            set { m_battleData.BattleFaction1Slot = value; }
        }

        public override int BattleFaction2Slot
        {
            get { return m_battleData.BattleFaction2Slot; }
            set { m_battleData.BattleFaction2Slot = value; }
        }

        public override bool BattleFaction1Ready
        {
            get { return m_battleData.BattleFaction1Ready; }
            set { m_battleData.BattleFaction1Ready = value; }
        }

        public override bool BattleFaction2Ready
        {
            get { return m_battleData.BattleFaction2Ready; }
            set { m_battleData.BattleFaction2Ready = value; }
        }

        public override int BattleTimeLimit
        {
            get { return m_battleData.BattleTimeLimit; }
            set { m_battleData.BattleTimeLimit = value; }
        }


        internal MyDedicatedServerBattle(IPEndPoint serverEndpoint)
            : base(new MySyncLayer(new MyTransportLayer(MyMultiplayer.GameEventChannel)))
        {
            RegisterControlMessage<ChatMsg>(MyControlMessageEnum.Chat, OnChatMessage);
            RegisterControlMessage<ServerBattleDataMsg>(MyControlMessageEnum.BattleData, OnServerBattleData);
            RegisterControlMessage<JoinResultMsg>(MyControlMessageEnum.JoinResult, OnJoinResult);

            Initialize(serverEndpoint);

            GameMode = MyGameModeEnum.Survival;

            m_battleData = new MyMultiplayerBattleData(this);
        }

        internal override void SendGameTagsToSteam()
        {
            if (SteamSDK.SteamServerAPI.Instance != null)
            {
                var serverName = MySandboxGame.ConfigDedicated.ServerName.Replace(":", "a58").Replace(";", "a59");

                Debug.Assert(GameMode == MyGameModeEnum.Survival);

                var gamemode = new StringBuilder();
                gamemode.Append("B");

                SteamSDK.SteamServerAPI.Instance.GameServer.SetGameTags(
                    "groupId" + m_groupId.ToString() +
                    " version" + MyFinalBuildConstants.APP_VERSION.ToString() +
                    " datahash" + MyDataIntegrityChecker.GetHashBase64() +
                    " " + MyMultiplayer.ModCountTag + ModCount +
                    " gamemode" + gamemode +
                    " " + MyMultiplayer.ViewDistanceTag + ViewDistance);
            }
        }

        protected override void SendServerData()
        {
            ServerBattleDataMsg msg = new ServerBattleDataMsg();
            msg.WorldName = m_worldName;
            msg.GameMode = m_gameMode;
            msg.HostName = m_hostName;
            msg.WorldSize = m_worldSize;
            msg.AppVersion = m_appVersion;
            msg.MembersLimit = m_membersLimit;
            msg.DataHash = m_dataHash;
            msg.BattleData = m_battleData.SaveData();

            SendControlMessageToAll(ref msg);
        }

        protected override void UserAccepted(ulong steamID)
        {
            // Battles - send all clients (note members without the accepted user), identities, players, factions as first message to client
            if (Battle || Scenario)
            {
                SendAllMembersDataToClient(steamID);
            }

            base.UserAccepted(steamID);
        }

        void OnChatMessage(ref ChatMsg msg, ulong sender)
        {
            if (m_memberData.ContainsKey(sender))
            {
                if (m_memberData[sender].IsAdmin)
                {
                    if (msg.Text.ToLower().Contains("+unban"))
                    {
                        string[] parts = msg.Text.Split(' ');
                        if (parts.Length > 1)
                        {
                            ulong user = 0;
                            if (ulong.TryParse(parts[1], out user))
                            {
                                BanClient(user, false);
                            }
                        }
                    }
                    else if (msg.Text.ToLower() == "+reload")
                    {
                        MySandboxGame.ReloadDedicatedServerSession();
                    }
                }
            }

            RaiseChatMessageReceived(sender, msg.Text, ChatEntryTypeEnum.ChatMsg);
        }

        void OnServerBattleData(ref ServerBattleDataMsg msg, ulong sender)
        {
            System.Diagnostics.Debug.Fail("None can send server data to server");
        }

        void OnJoinResult(ref JoinResultMsg msg, ulong sender)
        {
            System.Diagnostics.Debug.Fail("Server cannot join anywhere!");
        }


    }
}
