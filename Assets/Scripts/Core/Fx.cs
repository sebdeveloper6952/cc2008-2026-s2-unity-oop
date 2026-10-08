using System.Collections;
using UnityEngine;

// Ready-made effects: Fx.Laser(...), Fx.Spark(...), Fx.Explosion(...), Fx.Ping(...).
// Every laser also plays a zap, once per frame, so a shotgun blast sounds like one shot.
// No need to understand how they work inside.
public class Fx : MonoBehaviour
{
    static Fx instance;

    [SerializeField] Material laserMaterial;
    [SerializeField] Material particleMaterial;

    AudioSource speaker;
    AudioClip zap;
    int lastZapFrame = -1;

    void Awake()
    {
        instance = this;
        speaker = gameObject.AddComponent<AudioSource>();
        speaker.playOnAwake = false;
        zap = MakeZap();
    }

    public static void Laser(Vector3 from, Vector3 to)
    {
        var go = new GameObject("Laser");
        var line = go.AddComponent<LineRenderer>();
        line.material = instance.laserMaterial;
        line.positionCount = 2;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
        line.startWidth = 0.07f;
        line.endWidth = 0.03f;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        instance.StartCoroutine(FadeLine(line, 0.08f));
        instance.PlayZap();
    }

    public static void Spark(Vector3 at)
    {
        Burst(at, count: 10, speed: 3f, size: 0.12f, life: 0.25f,
              new Color(1f, 0.85f, 0.4f), new Color(1f, 0.4f, 0.1f), light: 0f);
    }

    public static void Explosion(Vector3 at, float scale = 1f)
    {
        Burst(at + Vector3.up * 0.5f, count: (int)(60 * scale), speed: 7f * scale, size: 0.7f * scale, life: 0.7f,
              new Color(1f, 0.8f, 0.3f), new Color(0.25f, 0.1f, 0.05f), light: 8f * scale);
    }

    // Small marker where the engineer was told to walk.
    public static void Ping(Vector3 at)
    {
        Burst(at + Vector3.up * 0.05f, count: 14, speed: 1.6f, size: 0.16f, life: 0.35f,
              new Color(0.4f, 0.9f, 1f), new Color(0.2f, 0.5f, 1f), light: 0f);
    }

    // ------------------------------------------------------------ internals

    // Six pellets in the same frame make one sound, not six.
    void PlayZap()
    {
        if (Time.frameCount == lastZapFrame) return;
        lastZapFrame = Time.frameCount;
        speaker.PlayOneShot(zap, 0.5f);
    }

    // A short laser zap built in code, so the project needs no audio files:
    // a tone that falls from 1700 Hz to 180 Hz, with a burst of noise at the start.
    static AudioClip MakeZap()
    {
        const int rate = 44100;
        int samples = (int)(rate * 0.16f);
        var data = new float[samples];
        var noise = new System.Random(13);
        float phase = 0f;
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            phase += 2f * Mathf.PI * Mathf.Lerp(1700f, 180f, Mathf.Sqrt(t)) / rate;
            float tone = Mathf.Sin(phase) + 0.3f * Mathf.Sign(Mathf.Sin(phase));
            float crack = t < 0.12f ? (float)(noise.NextDouble() * 2.0 - 1.0) * (1f - t / 0.12f) : 0f;
            float envelope = Mathf.Min(1f, i / (rate * 0.002f)) * Mathf.Exp(-5f * t);
            data[i] = 0.5f * envelope * (0.8f * tone + 0.4f * crack);
        }
        var clip = AudioClip.Create("Zap", samples, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static void Burst(Vector3 at, int count, float speed, float size, float life, Color start, Color end, float light)
    {
        var go = new GameObject("Burst");
        go.transform.position = at;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = life;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.5f, life);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.3f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
        main.gravityModifier = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(start, 0f), new GradientColorKey(end, 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0.2f));

        go.GetComponent<ParticleSystemRenderer>().material = instance.particleMaterial;
        ps.Play();

        if (light > 0f)
        {
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = start;
            l.range = 6f * (light / 8f);
            l.intensity = light;
            instance.StartCoroutine(FadeLight(l, life));
        }
        Destroy(go, life + 0.5f);
    }

    static IEnumerator FadeLine(LineRenderer line, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Destroy(line.gameObject);
    }

    static IEnumerator FadeLight(Light l, float seconds)
    {
        float start = l.intensity;
        for (float t = 0; t < seconds && l != null; t += Time.unscaledDeltaTime)
        {
            l.intensity = Mathf.Lerp(start, 0f, t / seconds);
            yield return null;
        }
    }
}
