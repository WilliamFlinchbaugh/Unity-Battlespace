using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class BattleEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo {
        public FlightAgent Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    public GameObject RedBase;
    public GameObject BlueBase;
    public SpectatorCamera SpectatorCam; // ADD RESET CAM !!

    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();
    [HideInInspector]
    public int LivingAgents;

    private SimpleMultiAgentGroup RedAgentGroup;
    private SimpleMultiAgentGroup BlueAgentGroup;

    private int ResetTimer;
    private GameObject Ground;
    [HideInInspector]
    public float FieldSize;
    private float SpawnRange;
    public float WinnerReward = 1;
    public float LoserPenalty = -1;
    public float HitPlaneReward = 1;
    public float HitBaseReward = 10;
    public float CrashPenalty = -0.05f;
    public float MissPenalty = 0f;
    public float FriendlyFirePenalty = -0.05f;
    public float FlightReward = 0.0001f;
    public float ShotRange = 600f;
    public float PlaneHealth = 2f;
    public float BaseHealth = 4f;
    public float ShotCooldown = 0.1f;

    void Start() {
        Ground = GameObject.Find("Ground");
        FieldSize = Ground.transform.localScale.x * 5;
        SpawnRange = FieldSize * 0.75f;

        RedAgentGroup = new SimpleMultiAgentGroup();
        BlueAgentGroup = new SimpleMultiAgentGroup();

        foreach (var item in AgentsList) {
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;

            if (item.Agent.team == Team.Red) {
                RedAgentGroup.RegisterAgent(item.Agent);
            } else {
                BlueAgentGroup.RegisterAgent(item.Agent);
            }
        }

        ResetScene();
    }

    void FixedUpdate() {
        ResetTimer += 1;

        if (LivingAgents == 0) {
            RedAgentGroup.EndGroupEpisode();
            BlueAgentGroup.EndGroupEpisode();
            ResetScene();
        }

        if (ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0) {
            RedAgentGroup.GroupEpisodeInterrupted();
            BlueAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

    public void BaseKilled(Team killedTeam) {
        if (killedTeam == Team.Blue) {
            RedAgentGroup.AddGroupReward(WinnerReward);
            BlueAgentGroup.AddGroupReward(LoserPenalty);
        } else {
            BlueAgentGroup.AddGroupReward(WinnerReward);
            RedAgentGroup.AddGroupReward(LoserPenalty);
        }
        RedAgentGroup.EndGroupEpisode();
        BlueAgentGroup.EndGroupEpisode();
        ResetScene();
    }

    public void Shot(FlightAgent shooter, GameObject target) {
        // Check if it is a base or an agent
        if (target.GetComponent<Base>() != null) { // Is a base
            Base baseScript = target.GetComponent<Base>();
            bool baseKilled = baseScript.Hit();

            if (baseScript.team == shooter.team) {
                shooter.AddReward(FriendlyFirePenalty);
            } else {
                shooter.AddReward(HitBaseReward);
            }
            if (baseKilled) {
                BaseKilled(baseScript.team);
            }
        } else if (target.GetComponent<FlightAgent>() != null) { // Is an agent
            FlightAgent agentScript = target.GetComponent<FlightAgent>();
            agentScript.Hit();

            if (agentScript.team == shooter.team) {
                shooter.AddReward(FriendlyFirePenalty);
            } else {
                shooter.AddReward(HitPlaneReward);
            }
        }
    }

    public void ResetScene() {
        ResetTimer = 0;
        LivingAgents = AgentsList.Count;

        //Reset Agents
        foreach (var item in AgentsList) {
            Vector3 newStartPos;
            Quaternion newRot;
            // quadrant 3
            if (item.Agent.team == Team.Red) {
                newStartPos = new Vector3(Random.Range(-SpawnRange, 0), 0.3f, Random.Range(-SpawnRange, 0));
                newRot = Quaternion.Euler(0, Random.Range(0, 90), 0);
            } else { // quadrant 1
                newStartPos = new Vector3(Random.Range(0, SpawnRange), 0.3f, Random.Range(0, SpawnRange));
                newRot = Quaternion.Euler(0, Random.Range(-180, -90), 0);
            }
            item.Agent.transform.position = newStartPos;
            item.Agent.transform.rotation = newRot;

            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
            item.Agent.gameObject.SetActive(true);
        }

        // Reset Bases
        Debug.Log(SpawnRange);
        RedBase.transform.position = new Vector3(Random.Range(-SpawnRange, 0), Random.Range(25f, 75f), Random.Range(-SpawnRange, 0));
        BlueBase.transform.position = new Vector3(Random.Range(0, SpawnRange), Random.Range(25f, 75f), Random.Range(0, SpawnRange));

        // Reset Camera
        SpectatorCam.ResetCam();
    }
}
