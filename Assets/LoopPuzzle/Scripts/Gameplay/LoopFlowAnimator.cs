using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LoopFlowAnimator : MonoBehaviour
{
    public GridManager gridManager;

    public float cellsPerSecond = 7f;
    public float laps = 2f;
    public float highlightWaveStepDelay = 0.06f;

    private GameObject glowDotGo;
    private TrailRenderer trail;
    private SpriteRenderer dotRenderer;
    private ParticleSystem burstParticles;

    private void Awake()
    {
        BuildGlowDot();
        BuildBurst();
    }

    public void Play(List<Cell> orderedCells, System.Action onComplete)
    {
        if (orderedCells == null || orderedCells.Count < 2)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(PlaySequence(orderedCells, onComplete));
    }

    private IEnumerator PlaySequence(List<Cell> orderedCells, System.Action onComplete)
    {
        List<Vector3> points = new List<Vector3>();
        foreach (Cell c in orderedCells)
        {
            points.Add(gridManager.GridToWorld(c.gridX, c.gridY));
        }

        yield return StartCoroutine(HighlightWave(orderedCells));

        yield return StartCoroutine(RunGlowDot(points));

        PlayBurst(points);

        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.45f, 0.35f, 14);
        if (HapticManager.Instance != null) HapticManager.Instance.HeavyTap();

        yield return new WaitForSeconds(0.45f);

        onComplete?.Invoke();
    }

    private IEnumerator HighlightWave(List<Cell> orderedCells)
    {
        foreach (Cell c in orderedCells)
        {
            if (c.currentPiece != null)
            {
                c.currentPiece.HighlightPulse();
            }
            yield return new WaitForSeconds(highlightWaveStepDelay);
        }
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator RunGlowDot(List<Vector3> points)
    {
        glowDotGo.SetActive(true);
        trail.Clear();

        float dotScale = gridManager.CellWorldSize * 0.4f;
        glowDotGo.transform.localScale = new Vector3(dotScale, dotScale, 1f);
        trail.startWidth = gridManager.CellWorldSize * 0.32f;
        trail.endWidth = 0f;

        glowDotGo.transform.position = points[0];

        if (SoundManager.Instance != null) SoundManager.Instance.PlayFlow();

        int n = points.Count;
        float totalSegments = n * laps;
        float segDuration = 1f / cellsPerSecond;

        for (int step = 0; step < totalSegments; step++)
        {
            Vector3 from = points[step % n];
            Vector3 to = points[(step + 1) % n];

            float elapsed = 0f;
            while (elapsed < segDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / segDuration);
                glowDotGo.transform.position = Vector3.Lerp(from, to, t);
                yield return null;
            }
        }

        glowDotGo.transform.position = points[0];

        dotRenderer.DOFade(0f, 0.25f);
        yield return new WaitForSeconds(0.25f);
        glowDotGo.SetActive(false);
        Color c = dotRenderer.color;
        c.a = 1f;
        dotRenderer.color = c;
    }

    private void BuildGlowDot()
    {
        glowDotGo = new GameObject("LoopGlowDot");
        glowDotGo.transform.SetParent(transform, false);

        dotRenderer = glowDotGo.AddComponent<SpriteRenderer>();
        dotRenderer.sprite = PieceSpriteFactory.GetGlowDotSprite();
        dotRenderer.sortingOrder = 20;

        trail = glowDotGo.AddComponent<TrailRenderer>();
        trail.time = 0.4f;
        trail.minVertexDistance = 0.02f;
        trail.numCapVertices = 6;
        trail.numCornerVertices = 6;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.sortingOrder = 19;

        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 1f);
        widthCurve.AddKey(1f, 0f);
        trail.widthCurve = widthCurve;
        trail.widthMultiplier = 0.32f;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.9f, 0.55f), 0f),
                new GradientColorKey(new Color(0.96f, 0.65f, 0.14f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.95f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        trail.colorGradient = gradient;

        glowDotGo.SetActive(false);
    }

    private void BuildBurst()
    {
        GameObject burstGo = new GameObject("LoopBurst");
        burstGo.transform.SetParent(transform, false);

        burstParticles = burstGo.AddComponent<ParticleSystem>();
        var main = burstParticles.main;
        main.duration = 1.2f;
        main.loop = false;
        main.startLifetime = 0.9f;
        main.startSpeed = 4.5f;
        main.startSize = 0.22f;
        main.startColor = new Color(1f, 0.82f, 0.35f, 1f);
        main.maxParticles = 120;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.15f;

        var emission = burstParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;

        var shape = burstParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.12f;

        var sizeOverLifetime = burstParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOverLifetime = burstParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0f),
                new GradientColorKey(new Color(0.961f, 0.651f, 0.137f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = grad;

        var renderer = burstGo.GetComponent<ParticleSystemRenderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        Sprite glow = PieceSpriteFactory.GetGlowDotSprite();
        if (glow != null) mat.mainTexture = glow.texture;
        renderer.material = mat;
        renderer.sortingOrder = 25;

        burstGo.SetActive(true);
    }

    private void PlayBurst(List<Vector3> points)
    {
        Vector3 center = Vector3.zero;
        foreach (Vector3 p in points) center += p;
        center /= points.Count;

        burstParticles.transform.position = center;
        burstParticles.Emit(90);
    }
}
