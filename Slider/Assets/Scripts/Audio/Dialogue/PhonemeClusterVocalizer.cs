using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SliderVocalization
{
    public class PhonemeClusterVocalizer : IVocalizer
    {
        public bool isVowelCluster;
        public bool isStressed = false;
        public string characters;

        public bool IsEmpty => characters.Length == 0;

        public int Progress => _progress;
        private int _progress = 0;
        public void ClearProgress() => _progress = 0;

        #region RANDOMIZED PARAMS
        float duration;
        float totalDuration;
        float wordIntonationMultiplier;
        float initialPitch;
        float finalPitch;
        float volumeAdjustmentDB;
        #endregion

        AudioManager.ManagedInstance playingInstance;

        public float RandomizeVocalization(VocalizerParameters parameters, VocalRandomizationContext context)
        {
            duration = parameters.duration * (context.isCurrentWordLow ? (1 - parameters.energeticWordSpeedup) : (1 + parameters.energeticWordSpeedup));
            totalDuration = duration * characters.Length;
            wordIntonationMultiplier = context.isCurrentWordLow ? (1 - parameters.wordIntonation) : (1 + parameters.wordIntonation);
            initialPitch = context.wordPitchBase * wordIntonationMultiplier;
            finalPitch = context.wordPitchIntonated * wordIntonationMultiplier;
            volumeAdjustmentDB = parameters.volumeAdjustmentDb;
            return totalDuration;
        }

        public IEnumerator Vocalize(VocalizerParameters parameters, VocalizationContext context, int idx, int lengthOfComposite)
        {
            ClearProgress();
            if (isStressed) parameters.ModifyWith(parameters.stressedVowelModifiers, createClone: false);

            float totalT = 0f;
            playingInstance = AudioManager.Play(parameters.synth
                .WithAttachmentToTransform(context.root)
                .WithFixedDuration(totalDuration)
                .WithVolume(parameters.volume)
                .WithParameter("Pitch", initialPitch)
                .WithParameter("VolumeAdjustmentDB", volumeAdjustmentDB)
                .WithParameter("VowelOpeness", context.vowelOpenness)
                .WithParameter("VowelForwardness", context.vowelForwardness),
                tick: delegate (ref EventInstance inst)
                {
                    inst.setParameterByName("Pitch", Mathf.Lerp(initialPitch, finalPitch, totalT / totalDuration));
                    inst.setParameterByName("VowelOpeness", context.vowelOpenness);
                    inst.setParameterByName("VowelForwardness", context.vowelForwardness);
                }
            );

            if (playingInstance == null) yield break;

            for (int i = 0; i < characters.Length; i++)
            {
                char c = characters[i];
                float t = 0;
                var vowelDescriptor = WordVocalizer.vowelDescriptionTable[c];

                _progress = i + 1;
                while (t < duration)
                {
                    // tick used to be called here, moved to AudioManager update function (see tick above)
                    context.vowelOpenness = Mathf.Lerp(context.vowelOpenness, vowelDescriptor.openness, t * parameters.lerpSmoothnessInverted);
                    context.vowelForwardness = Mathf.Lerp(context.vowelForwardness, vowelDescriptor.forwardness, t * parameters.lerpSmoothnessInverted);
                    t += Time.deltaTime;
                    totalT += Time.deltaTime;
                    yield return null;
                }
            }

            Stop();
        }

        public override string ToString()
        {
#if UNITY_EDITOR
            string text = $"<color=green>{characters.Substring(0, Progress)}</color>{characters.Substring(Progress)}";
            string pre = $"{(isVowelCluster ? "<B>" : "")}{(isStressed ? "<size=16>" : "")}";
            string post = $"{(isStressed ? "</size>" : "")}{(isVowelCluster ? "</B>" : "")}";
            return $"{pre}{text}{post}";
#else
            return characters;
#endif
        }

        public void Stop()
        {
            playingInstance?.Stop();
        }

    }
}