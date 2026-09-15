using System;
using System.IO;
using NUnit.Framework;
using TheCircussyOne.VisualTests.Editor;

public sealed class StageVisualCapturePlanTests
{
    private string temporaryDirectory;

    [SetUp]
    public void SetUp()
    {
        temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "circussy-stage-plan-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(temporaryDirectory))
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    [Test]
    public void LoadAcceptsStrictPlanAndResolvesPresentationCoordinates()
    {
        string path = WritePlan("captures/performer-jump/front.png");

        StageVisualCapturePlan plan = StageVisualCapturePlan.Load(path);

        Assert.That(plan.RehearsalId, Is.EqualTo("circussy-visual-rehearsal"));
        Assert.That(plan.Shots, Has.Count.EqualTo(1));
        StageVisualCaptureShot shot = plan.Shots[0];
        Assert.That(shot.SubjectId, Is.EqualTo("performer-jump"));
        Assert.That(shot.BackgroundId, Is.EqualTo("neutral-diagnostic"));
        Assert.That(shot.LightingId, Is.EqualTo("neutral-diagnostic"));
        Assert.That(shot.QualityId, Is.EqualTo("project-high"));
        Assert.That(shot.Width, Is.EqualTo(640));
        Assert.That(shot.Height, Is.EqualTo(360));
        Assert.That(shot.NormalizedTime, Is.EqualTo(0.5f));
        Assert.That(shot.StateScenarioTimeSeconds, Is.EqualTo(0.75f));
    }

    [Test]
    public void LoadRejectsArtifactTraversalBeforeSceneCapture()
    {
        string path = WritePlan("../outside.png");

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => StageVisualCapturePlan.Load(path));

        Assert.That(exception.Message, Does.Contain("must stay under the artifact root"));
    }

    [Test]
    public void ResolveContainedPathRejectsEscapingRoot()
    {
        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => StageVisualCapturePlan.ResolveContainedPath(temporaryDirectory, "../../outside.png"));

        Assert.That(exception.Message, Does.Contain("escapes root"));
    }

    private string WritePlan(string artifactPath)
    {
        string path = Path.Combine(temporaryDirectory, "capture-plan.json");
        string json =
            "{\n" +
            "  \"visual_capture_plan_version\": \"0.1\",\n" +
            "  \"rehearsal_id\": \"circussy-visual-rehearsal\",\n" +
            "  \"semantic_sha256\": \"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\",\n" +
            "  \"shots\": [\n" +
            "    {\n" +
            "      \"shot_id\": \"performer-jump-default-animation-front\",\n" +
            "      \"subject_id\": \"performer-jump\",\n" +
            "      \"subject_kind\": \"animation\",\n" +
            "      \"capture_mode\": \"animation\",\n" +
            "      \"state\": { \"id\": \"active\", \"adapter_state\": { \"scenario_time_seconds\": 0.75 } },\n" +
            "      \"view\": { \"azimuth_degrees\": 0, \"elevation_degrees\": 10, \"roll_degrees\": 0 },\n" +
            "      \"distance\": { \"bounds_multiplier\": 1.5, \"field_of_view_degrees\": 45 },\n" +
            "      \"resolution\": { \"width\": 640, \"height\": 360 },\n" +
            "      \"lighting\": { \"id\": \"neutral-diagnostic\" },\n" +
            "      \"quality\": { \"id\": \"project-high\" },\n" +
            "      \"adapter\": { \"background\": \"neutral-diagnostic\" },\n" +
            "      \"time\": { \"normalized_time\": 0.5 },\n" +
            $"      \"artifact_path\": \"{artifactPath}\"\n" +
            "    }\n" +
            "  ]\n" +
            "}\n";
        File.WriteAllText(path, json);
        return path;
    }
}
