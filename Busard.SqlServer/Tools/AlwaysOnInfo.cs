using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Busard.SqlServer.Tools
{
    internal enum HadrAvailabilityMode : byte
    {
        AsynchronousCommit = 0,
        SynchronousCommit = 1,
        ConfigurationOnly = 4
    }
    internal enum HadrFailoverMode : byte
    {
        Automatic = 0,
        Manual = 1
    }
    internal enum HadrPrimaryRoleAllowConnection : byte
    {
        All = 2,
        ReadWrite = 3
    }
    internal enum HadrSecondaryRoleAllowConnection : byte
    {
        No = 0,
        ReadOnly = 1,
        All = 2
    }

    internal enum HadrClusterMemberType : byte
    {
        WSFCNode = 0,
        DiskWitness = 1,
        FileShareWitness = 2,
        CloudWitness = 3
    }

    internal enum HadrMemberState : byte
    {
        Offline = 0,
        Online = 1
    }

    internal enum HadrRecoveryHealth : sbyte
    {
        Null = -1,
        InProgress = 0,
        Online = 1
    }

    internal enum HadrSynchronizationHealth : byte
    {
        NotHealthy = 0,
        PartiallyHealthy = 1,
        Healthy = 2
    }

    internal enum HadrQuorumType : byte
    {
        NodeMajority = 0,
        NodeAndDiskMajority = 1,
        NodeandFileShareMajority = 2,
        NoMajority = 3,
        UnknownQuorum = 4,
        CloudWitness = 5
    }

    internal enum HadrQuorumState : byte
    {
        Unknown = 0,
        Normal = 1,
        Forced = 2
    }

    /* -- xe map infos
    SELECT dxmv.name
	      ,dxmv.map_key
	      ,dxmv.map_value
          ,CONCAT(REPLACE(dxmv.map_value, '_', ''), ' = ', dxmv.map_key, ',') AS enum
    FROM sys.dm_xe_map_values dxmv
    WHERE dxmv.name LIKE 'hadr_%'
    ORDER BY dxmv.name, dxmv.map_key;
    */

    internal enum HadrAvailabilityReplicaRole : UInt32
    {
        RESOLVING_NORMAL = 0,
        RESOLVING_PENDING_FAILOVER = 1,
        PRIMARY_PENDING = 2,
        PRIMARY_NORMAL = 3,
        SECONDARY_NORMAL = 4,
        NOT_AVAILABLE = 5,
        GLOBAL_PRIMARY = 6,
        FORWARDER = 7,
        COUNT = 8
    }

    internal struct ClusterMember
    {
        public string Name { get; set; }
        public HadrClusterMemberType Type { get; set; }
        public HadrMemberState State { get; set; }
        public int NumberOfVotes { get; set; }
    }

    internal struct ClusterNode
    {
        public string ReplicaServerName { get; private set; }
        public string NodeName { get; private set; }
        public string JoinState { get; private set; }
        public bool IsPrimary { get; private set; }

        public ClusterNode(string replicaServerName, string nodeName, string joinState, bool isPrimary)
        {
            ReplicaServerName = replicaServerName ?? throw new ArgumentNullException(nameof(replicaServerName));
            NodeName = nodeName ?? throw new ArgumentNullException(nameof(nodeName));
            JoinState = joinState ?? throw new ArgumentNullException(nameof(joinState));
            IsPrimary = isPrimary;
        }
    }

    internal struct AvailabilityGroup
    {
        public string Name { get; set; }
        //public string ClusterType { get; set; }
        public Guid GroupId { get; set; }
        public HadrRecoveryHealth PrimaryHealth { get; set; }
        public HadrRecoveryHealth SecondaryHealth { get; set; }
        public HadrSynchronizationHealth SynchronizationHealth { get; set; }
        public IEnumerable<ClusterNode> Nodes { get; set; }
    }

    internal struct AvailabilityGroupReplica
    {
        public string ServerName { get; set; }
        public HadrAvailabilityMode AvailabilityMode { get; set; }
        public HadrFailoverMode FailoverMode { get; set; }
        public uint SessionTimeout { get; set; }

        public HadrPrimaryRoleAllowConnection PrimaryRoleAllowConnections { get; set; }
        public HadrSecondaryRoleAllowConnection SecondaryRoleAllowConnections { get; set; }
    }

    internal class AlwaysOnInfo
    {
        public string ClusterName { get; private set; }
        public HadrQuorumType QuorumType { get; private set; }
        public HadrQuorumState QuorumState { get; private set; }

        public List<ClusterMember> ClusterMembers { get; private set; }
        public List<AvailabilityGroup> AvailabilityGroups { get; private set; }

        public void GetDmHadrCluster()
        {
            using var cn = new SqlConnection(Configuration.ConnectionString.ConnectionString);
            using (var cmd = new SqlCommand(Resources.Queries.GetDmHadrCluster, cn))
            {
                cn.Open();
                using var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
                {
                    this.ClusterName = reader["cluster_name"].ToString();
                    this.QuorumType = (HadrQuorumType)reader.GetByte("quorum_type");
                    this.QuorumState = (HadrQuorumState)reader.GetByte("quorum_state");
                }
            }
            cn.Close();

        }

        public void GetClusterMembers()
        {
            using var cn = new SqlConnection(Configuration.ConnectionString.ConnectionString);
            using (var cmd = new SqlCommand(Resources.Queries.GetHadrClusterMembers, cn))
            {
                cn.Open();
                using var reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                while (reader.Read())
                {
                    var cm = new ClusterMember()
                    {
                        Name = reader["member_name"].ToString(),
                        Type = (HadrClusterMemberType)reader.GetByte("member_type"),
                        State = (HadrMemberState)reader.GetByte("member_state"),
                        NumberOfVotes = reader.GetInt32("number_of_quorum_votes")
                    };
                    this.ClusterMembers.Add(cm);
                }
            }
            cn.Close();
            
        }

        public void GetAvailabilityGroups()
        {
            this.AvailabilityGroups = new List<AvailabilityGroup>();
            using var cn = new SqlConnection(Configuration.ConnectionString.ConnectionString);

            // 1. getting groups
            using (var cmd = new SqlCommand(Resources.Queries.GetAvailabilityGroups, cn))
            {
                cn.Open();
                using var reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                while (reader.Read())
                {
                    var ag = new AvailabilityGroup()
                    {
                        GroupId = Guid.Parse(reader["group_id"].ToString()),
                        Name = reader["name"].ToString(),
                        PrimaryHealth = (HadrRecoveryHealth)reader.GetByte("primary_health"),
                        SecondaryHealth = (HadrRecoveryHealth)reader.GetByte("secondary_health"),
                        SynchronizationHealth = (HadrSynchronizationHealth)reader.GetByte("synchronization_health")
                    };
                    this.AvailabilityGroups.Add(ag);
                }
            }
            cn.Close();

            // 2. getting nodes
            //using (var cmd = new SqlCommand(Resources.Queries.GetHadrClusterNodes, cn))
            //{
            //    cn.Open();
            //    using var reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
            //    while (reader.Read())
            //    {
            //        var ag = new AvailabilityGroup()
            //        {
            //            GroupId = Guid.Parse(reader["group_id"].ToString()),
            //            Name = reader["name"].ToString() //,
            //            //Type = reader["cluster_type_desc"].ToString() TODO
            //        };
            //        this.AvailabilityGroups.Add(ag);
            //    }
            //}

            // 3. getting replicas

            //replica_server_name
            //availability_mode
            //failover_mode]
            //ar.session_timeout
            //primary_role_allow_connections
            //secondary_role_allow_connections

        }


    }
}
