using UnityEngine;

namespace Team5.qPCR
{
    [CreateAssetMenu(fileName = "PlateHandoffData", menuName = "Team 5/qPCR/Plate Handoff Data")]
    public sealed class PlateHandoffData : ScriptableObject
    {
        [SerializeField] private string plateId = "T4-QPCR-2026-05";
        [SerializeField] private bool filled = true;
        [SerializeField] private bool sealedPlate = true;
        [SerializeField] private bool bubbleFree = true;
        [SerializeField] private bool centrifuged = true;
        [SerializeField] private int activeReactionCount = 28;
        [SerializeField] private ReactionMixDefinition reactionMix;

        public string PlateId => plateId;
        public bool IsFilled => filled;
        public bool IsSealed => sealedPlate;
        public bool IsBubbleFree => bubbleFree;
        public bool IsCentrifuged => centrifuged;
        public int ActiveReactionCount => activeReactionCount;
        public ReactionMixDefinition ReactionMix => reactionMix;
        public bool IsReady => filled && sealedPlate && bubbleFree && centrifuged && activeReactionCount == 28
            && reactionMix != null && reactionMix.Validate(20f, "SYBR Green", out _);

        public void SetReactionMix(ReactionMixDefinition mix) => reactionMix = mix;

        public void Configure(string id, bool isFilled, bool isSealed, bool hasNoBubbles, bool isCentrifuged, int reactions)
        {
            plateId = id;
            filled = isFilled;
            sealedPlate = isSealed;
            bubbleFree = hasNoBubbles;
            centrifuged = isCentrifuged;
            activeReactionCount = reactions;
        }
    }
}
