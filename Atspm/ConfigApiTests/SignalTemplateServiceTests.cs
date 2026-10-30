using Utah.Udot.Atspm.ConfigApi.Services;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
namespace ConfigApiTests.Services;
[TestClass]
public class SignalTemplateServiceTests
{
    private static Location Site(params Approach[] approaches) => new() { Id = 1, LocationIdentifier = "Mock", Approaches = approaches.ToList() };
    private static IndianaEvent E(short code, short number) => new() { EventCode = code, EventParam = number };
    [DataTestMethod]
    [DataRow((short)32)] [DataRow((short)33)]
    public void FyaWarnsWithoutInferringProtectedService(short code)
    {
        var site = Site(new Approach { Id = 1, ProtectedPhaseNumber = 1, PermissivePhaseNumber = 2 });
        var r = SignalTemplateService.ModifyLocationWithEvents(site, new[] { E(code, 5), E(code, 5), E(1, 2) });
        Assert.AreEqual(1, r.ReconciliationWarnings.Count);
        StringAssert.Contains(r.ReconciliationWarnings[0], "identifiers 5");
        StringAssert.Contains(r.ReconciliationWarnings[0], "Review the left-turn configuration manually");
        Assert.AreEqual(1, site.Approaches.First().ProtectedPhaseNumber);
        Assert.AreEqual(0, r.LoggedButUnusedProtectedOrPermissivePhases.Count);
        Assert.AreEqual(0, r.LoggedButUnusedOverlapPhases.Count);
    }
    [DataTestMethod]
    [DataRow((short)61)] [DataRow((short)62)] [DataRow((short)63)]
    [DataRow((short)64)]
    public void VehicleOverlapEventsAreRecognized(short code)
    {
        var r = SignalTemplateService.ModifyLocationWithEvents(Site(), new[] { E(code, 3) });
        CollectionAssert.AreEqual(new short[] { 3 }, r.LoggedButUnusedOverlapPhases);
        Assert.AreEqual(0, r.LoggedButUnusedProtectedOrPermissivePhases.Count);
        Assert.AreEqual(0, r.ReconciliationWarnings.Count);
    }
    [DataTestMethod]
    [DataRow(false, (short)61)] [DataRow(true, (short)1)]
    public void OppositeNamespaceDoesNotValidateAssignment(bool overlap, short code)
    {
        var r = SignalTemplateService.ModifyLocationWithEvents(Site(new Approach { Id = 10, ProtectedPhaseNumber = 3, IsProtectedPhaseOverlap = overlap }), new[] { E(code, 3) });
        CollectionAssert.AreEqual(new[] { 10 }, r.RemovedApproachIds);
        Assert.AreEqual(0, r.ReconciliationWarnings.Count);
        Assert.IsTrue(r.MissingPhaseAssignments.Any(a => a.PhaseNumber == 3 && a.IsOverlap == overlap));
        CollectionAssert.AreEqual(new short[] { 3 }, overlap
            ? r.LoggedButUnusedProtectedOrPermissivePhases : r.LoggedButUnusedOverlapPhases);
    }
    [TestMethod]
    public void EachSlotUsesItsOwnOverlapFlag()
    {
        var site = Site(new Approach { Id = 1, ProtectedPhaseNumber = 1, PermissivePhaseNumber = 2, IsPermissivePhaseOverlap = true });
        var r = SignalTemplateService.ModifyLocationWithEvents(site, new[] { E(1, 1), E(1, 2), E(61, 1), E(61, 2) });
        CollectionAssert.AreEqual(new short[] { 2 }, r.LoggedButUnusedProtectedOrPermissivePhases);
        CollectionAssert.AreEqual(new short[] { 1 }, r.LoggedButUnusedOverlapPhases);
        Assert.AreEqual(0, r.RemovedApproachIds.Count);
        Assert.AreEqual(0, r.ReconciliationWarnings.Count);
    }
    [DataTestMethod]
    [DataRow((short)67)] [DataRow((short)68)] [DataRow((short)69)]
    public void PedestrianOverlapsAreNotOfferedAsVehiclePhases(short code)
    {
        var r = SignalTemplateService.ModifyLocationWithEvents(Site(), new[] { E(code, 4) });
        Assert.AreEqual(0, r.LoggedButUnusedOverlapPhases.Count);
        StringAssert.Contains(r.ReconciliationWarnings.Single(), "Pedestrian overlap 4");
        r = SignalTemplateService.ModifyLocationWithEvents(Site(new Approach { Id = 1, PedestrianPhaseNumber = 4, IsPedestrianPhaseOverlap = true }), new[] { E(code, 4) });
        Assert.AreEqual(0, r.RemovedApproachIds.Count);
        Assert.AreEqual(0, r.ReconciliationWarnings.Count);
    }
    [TestMethod]
    public void PedestrianAndVehicleOverlapWithSameNumberRemainSeparate()
    {
        var r = SignalTemplateService.ModifyLocationWithEvents(Site(new Approach { Id = 1, PedestrianPhaseNumber = 4, IsPedestrianPhaseOverlap = true }), new[] { E(67, 4), E(61, 4) });
        CollectionAssert.AreEqual(new short[] { 4 }, r.LoggedButUnusedOverlapPhases);
    }
    [TestMethod]
    public void DetectorActivityStillRetainsApproachWithoutPhaseTransitions()
    {
        var r = SignalTemplateService.ModifyLocationWithEvents(Site(new Approach
        {
            Id = 1, ProtectedPhaseNumber = 2,
            Detectors = new List<Detector> { new() { Id = 1, DetectorChannel = 118 } }
        }), new[] { E(82, 118) });
        Assert.AreEqual(0, r.RemovedApproachIds.Count);
        Assert.AreEqual(0, r.RemovedDetectors.Count);
        Assert.AreEqual(0, r.LoggedButUnusedDetectorChannels.Count);
    }

    [TestMethod]
    public void MissingAssignmentsAreIndependentAndDoNotMutateConfiguration()
    {
        var approach = new Approach { Id = 1, ProtectedPhaseNumber = 1,
            PermissivePhaseNumber = 2, IsPermissivePhaseOverlap = true,
            PedestrianPhaseNumber = 3, IsPedestrianPhaseOverlap = true,
            Detectors = new List<Detector> { new() { Id = 1, DetectorChannel = 118 } } };
        var result = SignalTemplateService.ModifyLocationWithEvents(Site(approach), new[] { E(1, 1), E(82, 118) });
        Assert.AreEqual(0, result.RemovedApproachIds.Count);
        Assert.AreEqual(2, result.MissingPhaseAssignments.Count);
        Assert.IsTrue(result.MissingPhaseAssignments.Any(a => a.ApproachId == 1 && a.Role == "permissive" && a.PhaseNumber == 2 && a.IsOverlap));
        Assert.IsTrue(result.MissingPhaseAssignments.Any(a => a.Role == "pedestrian" && a.PhaseNumber == 3 && a.IsOverlap));
        Assert.AreEqual(2, approach.PermissivePhaseNumber);
        Assert.AreEqual(1, approach.Detectors.Count);
    }

    [TestMethod]
    public void ZeroAndNullAreNotMissingAssignments()
    {
        var result = SignalTemplateService.ModifyLocationWithEvents(Site(new Approach { Id = 1, ProtectedPhaseNumber = 0 }), Array.Empty<IndianaEvent>());
        Assert.AreEqual(0, result.MissingPhaseAssignments.Count);
    }

    [DataTestMethod]
    [DataRow((short)21)] [DataRow((short)22)] [DataRow((short)23)]
    public void PedestrianEventsValidateOnlyPedestrianAssignments(short code)
    {
        var site = Site(new Approach { Id = 1, ProtectedPhaseNumber = 4, PedestrianPhaseNumber = 4 });
        var result = SignalTemplateService.ModifyLocationWithEvents(site, new[] { E(code, 4) });
        Assert.IsFalse(result.MissingPhaseAssignments.Any(a => a.Role == "pedestrian"));
        Assert.IsTrue(result.MissingPhaseAssignments.Any(a => a.Role == "protected"));
    }
    [TestMethod]
    public void OneToOnePedestrianMismatchIsFlaggedWithoutChangingConfiguration()
    {
        var site = Site(new Approach { Id = 1, ProtectedPhaseNumber = 2, PedestrianPhaseNumber = 4 });
        site.PedsAre1to1 = true;
        var result = SignalTemplateService.ModifyLocationWithEvents(site, new[] { E(1, 2), E(21, 4) });
        Assert.IsTrue(result.ReconciliationWarnings.Any(w => w.Contains("one-to-one")));
        Assert.AreEqual(4, site.Approaches.First().PedestrianPhaseNumber);
    }

    [TestMethod]
    public void PedestrianDetectorDoesNotValidatePedestrianPhase()
    {
        var result = SignalTemplateService.ModifyLocationWithEvents(Site(new Approach { Id = 1, ProtectedPhaseNumber = 2, PedestrianPhaseNumber = 4 }), new[] { E(90, 4), E(1, 2) });
        Assert.IsTrue(result.MissingPhaseAssignments.Any(a => a.Role == "pedestrian"));
        Assert.AreEqual(0, result.LoggedButUnusedPedestrianPhases.Count);
    }

    [DataTestMethod]
    [DataRow((short)65)] [DataRow((short)66)] [DataRow((short)70)]
    public void OffAndDarkEventsDoNotEstablishActiveOverlaps(short code)
    {
        var result = SignalTemplateService.ModifyLocationWithEvents(Site(), new[] { E(code, 3) });
        Assert.AreEqual(0, result.LoggedButUnusedOverlapPhases.Count);
        Assert.AreEqual(0, result.ReconciliationWarnings.Count);
        var configured = Site(new Approach { Id = 1, ProtectedPhaseNumber = 3, IsProtectedPhaseOverlap = true,
            PedestrianPhaseNumber = 3, IsPedestrianPhaseOverlap = true });
        result = SignalTemplateService.ModifyLocationWithEvents(configured, new[] { E(code, 3) });
        Assert.AreEqual(2, result.MissingPhaseAssignments.Count);
    }
    [TestMethod]
    public void OneToOneNullPedestrianPhaseUsesProtectedFallback()
    {
        var site = Site(new Approach { Id = 1, ProtectedPhaseNumber = 2 });
        site.PedsAre1to1 = true;
        var result = SignalTemplateService.ModifyLocationWithEvents(site, new[] { E(1, 2) });
        Assert.IsFalse(result.ReconciliationWarnings.Any(w => w.Contains("one-to-one")));
    }

    [DataTestMethod]
    [DataRow(true, false)]
    [DataRow(false, false)]
    [DataRow(true, true)]
    public void NullPedestrianAssignmentOnlyConsumesInheritedRegularPhase(bool oneToOne, bool protectedOverlap)
    {
        var site = Site(new Approach { Id = 1, ProtectedPhaseNumber = 2, IsProtectedPhaseOverlap = protectedOverlap });
        site.PedsAre1to1 = oneToOne;
        // Pedestrian-only evidence must also retain the approach when inherited.
        var result = SignalTemplateService.ModifyLocationWithEvents(site, new[] { E(21, 2) });
        var inherited = oneToOne && !protectedOverlap;
        Assert.AreEqual(inherited ? 0 : 1, result.LoggedButUnusedPedestrianPhases.Count);
        Assert.AreEqual(!inherited, result.ReconciliationWarnings.Any(w => w.Contains("without a configured pedestrian")));
        Assert.AreEqual(inherited ? 0 : 1, result.RemovedApproachIds.Count);
    }
}
