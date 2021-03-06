using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntitySpellController))]
[RequireComponent(typeof(MageRLParameters))]
public class Mage : AbstractEntity
{
    [Header("Mage references")]
    [SerializeField] private EntitySpellController spellController;
    [SerializeField] private MageRLParameters rlParameters;

    [Header("Mana")]
    public float maxMana;
    public float manaRestoreRate;

    [Header("Spell timings")]
    public float maxShieldTime;
    public float maxHealTime;

    [Header("Power spells probability")]
    [Range(0, 1)]
    public float coverHealProbability;
    [Range(0, 1)]
    public float powerAreaSpellCastProbability;

    [Header("Behaviour tree time clock")]
    public float startTreeTime;
    public float repeatTreeTime;


    /*>>> Unity methods <<<*/
    protected override void Awake()
    {
        base.Awake();
        spellController = GetComponent<EntitySpellController>();
        rlParameters = GetComponent<MageRLParameters>();
    }

    protected override void Start()
    {
        base.Start();
        Managers.Level.ValidateLevelEntities(gameObject);

        ConstructBehaviourTree();
        InvokeRepeating("EvaluateBehaviourTree", startTreeTime, repeatTreeTime);
    }


    /*Behaviour tree methods*/
    private void EvaluateBehaviourTree()
    {
        decisionTreeTopNode.Evaluate();
    }

    protected override void ConstructBehaviourTree()
    {
        /*>>> Cover branch <<<*/
        /*Cover level 6*/
        IsCoverAvaliableNode coverAvaliableNode = new IsCoverAvaliableNode(this, avaliableCovers, enemy.transform, this.transform, new GetFloatValue(() => sightConeRange));
        GoToDestinationPoint goToCoverNode = new GoToDestinationPoint(this, this.transform, new GetFloatValue[] {
            new GetFloatValue(() => runSpeed),
            new GetFloatValue(() => restSpeed),
            new GetFloatValue(() => accelerationChaseBonus),
            new GetFloatValue(() => acceleration)
        });
        HealSpellExecuteNode healSpellExecuteNode = new HealSpellExecuteNode(this, new GetFloatValue(() => coverHealProbability));

        /*Cover level 5*/
        // Go to cover (Sequence)
        // <Safe jump> => Sense decisions (Selector)
        ChangeSpeedNode restInCoverNode = new ChangeSpeedNode(this, new GetFloatValue[] {
            new GetFloatValue(() => (restSpeed + 0.5f)),
            new GetFloatValue(() => breakAcceleration)
        });
        IsInDestinationPoint gotIntoCover = new IsInDestinationPoint(this, this.transform);
        IsDirectContactNode isCoveredNode = new IsDirectContactNode(enemy.transform, this.transform);


        /*Cover level 4*/
        // Is entity covered (Sequence)
        // Find cover (Selector)

        /*Cover level 3*/
        HealthNode healthNode = new HealthNode(new GetFloatValue[] {
            new GetFloatValue(() => health),
            new GetFloatValue(() => lowHealthThreshold)
        });
        // Try to take cover (Selector)

        /*Cover level 2*/
        // Low health (Sequence)


        //===================================================================


        /*>>> Panic branch <<<*/
        /*Panic level 4*/
        AreaExplosionExecuteNode areaExplosionNode = new AreaExplosionExecuteNode(this, new GetFloatValue(() => powerAreaSpellCastProbability));
        // <Safe jump> => Try to take cover (Selector)

        /*Panic level 3*/
        HealthNode criticHealthNode = new HealthNode(new GetFloatValue[] {
            new GetFloatValue(() => health),
            new GetFloatValue(() => criticalLowHealthThreshold)
        });
        // Panic reaction (Sequence)

        /*Panic level 2*/
        // Panic (Sequence)


        //===================================================================


        /*>>> Attact branch <<<*/
        /*Attack level 4*/
        RangeNode attackingRangeNode = new RangeNode(enemy.transform, this.transform, new GetFloatValue(() => attackRange));
        IsDirectContactNode isPlayerCovered = new IsDirectContactNode(enemy.transform, this.transform);
        Inverter isPlayerNotCovered = new Inverter(isPlayerCovered);

        /*Attack level 3*/
        // Clear spot (Sequence)
        AttackNode attackNode = new AttackNode(this, enemy.transform, this.transform, new GetFloatValue[] {
            new GetFloatValue(() => restSpeed),
            new GetFloatValue(() => breakAcceleration)
        });

        /*Attack level 2*/
        // Attack (Sequence)


        //===================================================================


        /*>>> Chanse branch <<<*/
        /*Chase level 4*/
        RangeNode hearRangeNode = new RangeNode(enemy.transform, this.transform, new GetFloatValue(() => hearRange));
        SightNode sightNode = new SightNode(enemy.transform, this.transform, new GetFloatValue[] {
            new GetFloatValue(() => sightRange),
            new GetFloatValue(() => sightConeRange)
        });

        /*Chase level 3*/
        // Senses (Selector)
        ChaseNode chaseNode = new ChaseNode(this, enemy.transform, new GetFloatValue[] {
            new GetFloatValue(() => runSpeed),
            new GetFloatValue(() => restSpeed),
            new GetFloatValue(() => accelerationChaseBonus),
            new GetFloatValue(() => acceleration)
        });

        /*Chase level 2*/
        // Chase (Sequence)


        //===================================================================


        /*>>> Wander branch <<<*/
        /*Wander level 2*/
        WanderNode wanderNode = new WanderNode(this, this.transform, new GetFloatValue[] {
            new GetFloatValue(() => restSpeed),
            new GetFloatValue(() => acceleration),
            new GetFloatValue(() => walkPointRange),
            new GetFloatValue(() => minRestTime),
            new GetFloatValue(() => maxRestTime),
            new GetFloatValue(() => turnMaxAngle)
        });
        GoToDestinationPoint goToWanderPointNode = new GoToDestinationPoint(this, this.transform, new GetFloatValue[] {
            new GetFloatValue(() => walkSpeed),
            new GetFloatValue(() => restSpeed),
            new GetFloatValue(() => acceleration),
            new GetFloatValue(() => acceleration)
        });


        //===================================================================


        /*>>> Safe jump nodes <<<*/
        SafeJumpNode jumpToTryToTakeCoverSelector = new SafeJumpNode();
        SafeJumpNode jumpToSenseDecisionsSelector = new SafeJumpNode();


        /*>>> Health decisions <<<*/
        Sequence goToCoverSequence = new Sequence(new List<Node> { coverAvaliableNode, goToCoverNode, healSpellExecuteNode });
        Selector findCoverSelector = new Selector(new List<Node> { goToCoverSequence, jumpToSenseDecisionsSelector });
        Sequence isEntityCoveredSequence = new Sequence(new List<Node> { isCoveredNode, gotIntoCover, restInCoverNode });
        Selector tryToTakeCoverSelector = new Selector(new List<Node> { isEntityCoveredSequence, findCoverSelector });
        Sequence lowHealthSequence = new Sequence(new List<Node> { healthNode, tryToTakeCoverSelector });

        Sequence panicReactionSequence = new Sequence(new List<Node> { areaExplosionNode, jumpToTryToTakeCoverSelector });
        Sequence criticalLowHealthSequence = new Sequence(new List<Node> { criticHealthNode, panicReactionSequence });


        /*>>> Sense decisions <<<*/
        Selector sensesSelector = new Selector(new List<Node> { hearRangeNode, sightNode });
        Sequence chaseSequence = new Sequence(new List<Node> { sensesSelector, chaseNode });

        Sequence clearSpot = new Sequence(new List<Node> { attackingRangeNode, isPlayerNotCovered });
        Sequence attackSequence = new Sequence(new List<Node> { clearSpot, attackNode });


        /*>>> Top level decisions <<<*/
        Selector healthDecisionsSelector = new Selector(new List<Node> { criticalLowHealthSequence, lowHealthSequence });
        Selector senseDecisionsSelector = new Selector(new List<Node> { attackSequence, chaseSequence });
        Selector wanderSelector = new Selector(new List<Node> { wanderNode, goToWanderPointNode });


        /*Setup safe jumps*/
        jumpToTryToTakeCoverSelector.SetJumpNode(tryToTakeCoverSelector);
        jumpToSenseDecisionsSelector.SetJumpNode(senseDecisionsSelector);


        decisionTreeTopNode = new Selector(new List<Node> { healthDecisionsSelector, senseDecisionsSelector, wanderSelector });
    }



    /*>>> Hurting / Die methods <<<*/
    public override void Die()
    {
        Managers.Level.EndEpisode(gameObject);
    }

    public override void GetHit(float damage)
    {
        ChangeHealth(-damage);

        animationController.PlayGetHitAnimation();
        AddRLReward(rlParameters.getHurt);
        Debug.Log("[Phisical hit] Health: " + health);
    }

    public override void GetMagicHit(float damage, int spellID)
    {
        ChangeHealth(-damage);

        animationController.PlayGetHitAnimation();
        AddRLReward(rlParameters.getHurt);
        spellController.SetLastHittedSpellID(spellID);
        Debug.Log("[Magic hit] Health: " + health);
    }


    /*>>> Attack methods <<<*/
    public override void Attack()
    {
        spellController.ExecuteSpell();
    }

    public void SetSpellType(SpellType spellType, int spellID)
    {
        try
        {
            spellController.SetSpellType(spellType, spellID);
        }
        catch (UnknownSpellException err)
        {
            Debug.LogError(err);
        }
    }


    /*>>> Getters <<<*/
    public EntitySpellController GetSpellController()
    {
        return spellController;
    }

    public MageRLParameters GetMageRLParameters()
    {
        return rlParameters;
    }
}
