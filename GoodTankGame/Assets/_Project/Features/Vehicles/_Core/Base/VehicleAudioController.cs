using UnityEngine;

public class VehicleAudioController : MonoBehaviour
{
    [Space(15)]
    [Header("ENGINE")]
    [SerializeField] private AudioClip idle_Clip;
    [SerializeField] private AudioClip midRPM_Clip;
    [SerializeField] private AudioClip highRPM_Clip;
    [Space(5)]
    [SerializeField] private float basePitch = 0.8f;
    [SerializeField] private float maxPitch = 1.5f;
    [SerializeField] private float volumeLerpSpeed = 5f;

    private AudioSource idleSource;
    private AudioSource HighRPMSource;
    private AudioSource MidSource;



    private void Awake()
    {
        SetupEngineAudio(ref idleSource, idle_Clip);
        SetupEngineAudio(ref HighRPMSource, highRPM_Clip);
        SetupEngineAudio(ref MidSource, midRPM_Clip);

      
    }

    #region ENGINE
    private void SetupEngineAudio(ref AudioSource source, AudioClip clip)
    {
        source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        //source.outputAudioMixerGroup
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0.9f;
        source.volume = 0f;
        source.Play();
    }
    public void ChangeEngineAudio(float RPM_Normalized, float engineLoad)
    {
        RPM_Normalized = Mathf.Clamp01(RPM_Normalized);
        float currentPitch = Mathf.Lerp(basePitch, maxPitch, RPM_Normalized);
        engineLoad = Mathf.Clamp01(engineLoad);

        idleSource.pitch = currentPitch;
        MidSource.pitch = currentPitch;
        HighRPMSource.pitch =  Mathf.Clamp(currentPitch, 0f, 1f);

        float idleTarget = 0f;
        float midTarget = 0f;
        float highTarget = 0f;


        if (RPM_Normalized < 0.5f)
        {
            float t = RPM_Normalized / 0.5f;

            // Equal-power crossfade idle -> mid
            idleTarget = Mathf.Cos(t * Mathf.PI * 0.5f);
            midTarget = Mathf.Sin(t * Mathf.PI * 0.5f);
            highTarget = 0f;
        }
        else
        {
            float t = (RPM_Normalized - 0.5f) / 0.5f;

            // Equal-power crossfade mid -> high
            idleTarget = 0f;
            midTarget = Mathf.Cos(t * Mathf.PI * 0.5f);
            highTarget = Mathf.Sin(t * Mathf.PI * 0.5f);
        }


        // --- wygładzanie głośności ---
        float dt = Time.deltaTime;
        if (idleSource != null)
            idleSource.volume = Mathf.Lerp(idleSource.volume, idleTarget, 1f - Mathf.Exp(-volumeLerpSpeed * dt));
        if (MidSource != null)
            MidSource.volume = Mathf.Lerp(MidSource.volume, midTarget, 1f - Mathf.Exp(-volumeLerpSpeed * dt));
        if (HighRPMSource != null)
            HighRPMSource.volume = Mathf.Lerp(HighRPMSource.volume, highTarget, 1f - Mathf.Exp(-volumeLerpSpeed * dt));

    }
    #endregion

}
