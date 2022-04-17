﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.ComponentDetection.Common.DependencyGraph;
using Microsoft.ComponentDetection.Contracts;
using Microsoft.ComponentDetection.Contracts.TypedComponent;
using Microsoft.ComponentDetection.Detectors.Pub;
using Microsoft.ComponentDetection.Detectors.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.ComponentDetection.TestsUtilities;

namespace Microsoft.ComponentDetection.Detectors.Tests
{
    [TestClass]
    [TestCategory("Governance/All")]
    [TestCategory("Governance/ComponentDetection")]
    public class PubDetectorTest
    {
        private Mock<ILogger> loggerMock;
        private PubComponentDetector pubDetector;
        private DetectorTestUtility<PubComponentDetector> detectorTestUtility;

        [TestInitialize]
        public void TestInitialize()
        {
            loggerMock = new Mock<ILogger>();
            pubDetector = new PubComponentDetector
            {
                Logger = loggerMock.Object,
            };
            detectorTestUtility = DetectorTestUtilityCreator.Create<PubComponentDetector>()
                                    .WithScanRequest(new ScanRequest(
                                        new DirectoryInfo(Path.GetTempPath()), null, null, new Dictionary<string, string>(), null,
                                        new ComponentRecorder(enableManualTrackingOfExplicitReferences: !pubDetector.NeedsAutomaticRootDependencyCalculation)));
        }

        [TestMethod]
        public async Task TestPubDetector_TestMultipleLockfiles()
        {
            var pubLockFile = @"# Generated by pub
# See https://dart.dev/tools/pub/glossary#lockfile
packages:
  _fe_analyzer_shared:
    dependency: transitive
    description:
      name: _fe_analyzer_shared
      url: ""https://pub.dartlang.org""
    source: hosted
    version: ""38.0.0""
  aligned_dialog:
    dependency: ""direct main""
    description:
      name: aligned_dialog
      url: ""https://pub.dartlang.org""
    source: hosted
    version: ""0.0.6""
  analyzer:
    dependency: transitive
    description:
      name: analyzer
      url: ""https://pub.dartlang.org""
    source: hosted
    version: ""3.4.1""
  archive:
    dependency: transitive
    description:
      name: archive
      url: ""https://pub.dartlang.org""
    source: hosted
    version: ""3.3.0""
  args:
    dependency: transitive
    description:
      name: args
      url: ""https://pub.dartlang.org""
    source: hosted
    version: ""2.3.0""
  async:
    dependency: transitive
    description:
      name: async
      url: ""https://pub.dartlang.org""
    source: hosted
    version: ""2.8.2""
  boolean_selector:
    dependency: transitive
    description:
      name: boolean_selector
      url: ""https://pub.dartlang.org""
    source: hosted
    version: ""2.1.0""
sdks:
  dart: "">=2.15.0 <3.0.0""
  flutter: "">=2.10.0""";

            var (scanResult, componentRecorder) = await detectorTestUtility
                                                    .WithFile("pubspec.lock", pubLockFile)
                                                    .ExecuteDetector();

            Assert.AreEqual(ProcessingResultCode.Success, scanResult.ResultCode);

            var detectedComponents = componentRecorder.GetDetectedComponents();
            Assert.AreEqual(7, detectedComponents.Count());

            AssertPubComponentNameAndVersion(detectedComponents, "_fe_analyzer_shared", "38.0.0");
            AssertPubComponentNameAndVersion(detectedComponents, "aligned_dialog", "0.0.6");
            AssertPubComponentNameAndVersion(detectedComponents, "analyzer", "3.4.1");
            AssertPubComponentNameAndVersion(detectedComponents, "archive", "3.3.0");
            AssertPubComponentNameAndVersion(detectedComponents, "args", "2.3.0");
            AssertPubComponentNameAndVersion(detectedComponents, "async", "2.8.2");
            AssertPubComponentNameAndVersion(detectedComponents, "boolean_selector", "2.1.0");
        }

        private void AssertPubComponentNameAndVersion(IEnumerable<DetectedComponent> detectedComponents, string name, string version)
        {
            Assert.IsNotNull(
                detectedComponents.SingleOrDefault(c =>
            c.Component is RubyGemsComponent component &&
            component.Name.Equals(name) &&
            component.Version.Equals(version)), $"Component with name {name} and version {version} was not found");
        }
    }
}
