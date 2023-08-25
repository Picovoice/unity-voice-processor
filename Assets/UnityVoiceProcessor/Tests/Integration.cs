using NUnit.Framework;
using System.Threading;

using Pv.Unity;

namespace Tests
{

    public class Integration
    {
        private readonly int _frameLength = 512;
        private readonly int _sampleRate = 16000;

        [Test]
        public void TestGetInstance()
        {
            VoiceProcessor vp = VoiceProcessor.Instance;
            Assert.IsNotNull(vp);
        }

        [Test]
        public void TestBasic()
        {
            VoiceProcessor vp = VoiceProcessor.Instance;
            Assert.IsNotNull(vp);

            vp.AddFrameListener((short[] frame) =>
            {
                Assert.AreEqual(vp.FrameLength, frame.Length);
            });

            Assert.IsFalse(vp.IsRecording);
            vp.StartRecording(_frameLength, _sampleRate);
            Assert.IsTrue(vp.IsRecording);

            Thread.Sleep(1000);

            vp.StopRecording();

            Assert.Greater(vp.NumFrameListeners, 0);
            Assert.IsFalse(vp.IsRecording);

            vp.ClearFrameListeners();
        }

        [Test]
        public void TestAddRemoveListeners()
        {
            VoiceProcessor vp = VoiceProcessor.Instance;
            Assert.IsNotNull(vp);

            VoiceProcessorFrameListener b1 = frame => { };
            VoiceProcessorFrameListener b2 = frame => { };

            vp.AddFrameListener(b1);
            Assert.AreEqual(vp.NumFrameListeners, 1);
            vp.AddFrameListener(b2);
            Assert.AreEqual(vp.NumFrameListeners, 2);
            vp.RemoveFrameListener(b1);
            Assert.AreEqual(vp.NumFrameListeners, 1);
            vp.RemoveFrameListener(b1);
            Assert.AreEqual(vp.NumFrameListeners, 1);
            vp.RemoveFrameListener(b2);
            Assert.AreEqual(vp.NumFrameListeners, 0);

            VoiceProcessorFrameListener[] bs = new VoiceProcessorFrameListener[] { b1, b2 };
            vp.AddFrameListeners(bs);
            Assert.AreEqual(vp.NumFrameListeners, 2);
            vp.RemoveFrameListeners(bs);
            Assert.AreEqual(vp.NumFrameListeners, 0);
            vp.AddFrameListeners(bs);
            Assert.AreEqual(vp.NumFrameListeners, 2);
            vp.ClearFrameListeners();
            Assert.AreEqual(vp.NumFrameListeners, 0);
        }
    }
}