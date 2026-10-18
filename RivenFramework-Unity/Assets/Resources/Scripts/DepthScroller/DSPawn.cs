//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
    
public class DSPawn : Pawn
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [HideInInspector] public List<Pawn> visiblePawns = new List<Pawn>();
    [HideInInspector] public List<Pawn> visibleHostiles = new List<Pawn>();
    [HideInInspector] public List<Pawn> visibleAllies = new List<Pawn>();
    public bool WantsToDrop;


    //=-----------------=
    // Private Variables
    //=-----------------=
    public GameObject characterVisuals;
    public ParticleSystem hurtParticleEmitter;

    public List<SpriteRenderer> spritesRenderers;
    public Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
    public float flashDuration = 0.1f;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    // These values are cast back to the base pawn classes currentStats and defaultStats
    public DSPawnStats DSDefaultStats;
    public DSPawnStats DSCurrentStats => (DSPawnStats)currentStats;
    public DSPawnActions DSaction => (DSPawnActions)action;
    
    [HideInInspector] public Rigidbody physicsbody;
    [SerializeField] public GameObject interactionPrefab;
    public Animator animator;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public void Awake()
    {
        // Get references
        physicsbody = GetComponent<Rigidbody>();
        viewPoint = transform.Find("ViewPoint");
        animator = GetComponentInChildren<Animator>();

        defaultStats = DSDefaultStats;
        currentStats = (DSPawnStats)DSDefaultStats.Clone(); // Don't forget to clone so that you don't overwrite the pawns default values! ~Liz
        action = DSaction;
        
        spritesRenderers = GetComponentsInChildren<SpriteRenderer>(true).ToList();
        foreach (var r in spritesRenderers) originalColors[r] = r.color;
    }
    

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}