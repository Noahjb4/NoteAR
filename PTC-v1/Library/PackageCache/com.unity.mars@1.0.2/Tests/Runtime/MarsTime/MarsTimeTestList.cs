#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.MARS.Tests
{
    public class MarsTimeTestList
    {
        MarsTimeModule m_MarsTimeModule;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MarsTimeModule = MarsTimeModule.instance;
            m_MarsTimeModule.LoadModule();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_MarsTimeModule.UnloadModule();
        }

        [SetUp]
        public void SetUp()
        {
            // Must set start time here so tests can know the difference in Time.time from when MarsTimeModule was set up
            MarsTimeTest.StartTime = Time.time;
            m_MarsTimeModule.OnBehaviorEnable();
        }

        [TearDown]
        public void TearDown()
        {
            m_MarsTimeModule.OnBehaviorDisable();
        }

        [UnityTest]
        public IEnumerator MarsTimeTick() { yield return new MonoBehaviourTest<MarsTimeTickTest>(); }

        [UnityTest]
        public IEnumerator MarsTimeScale() { yield return new MonoBehaviourTest<MarsTimeScaleTest>(); }
    }
}
#endif
