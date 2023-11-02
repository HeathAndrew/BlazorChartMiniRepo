using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTestApp.Shared
{
    public class SineWaveDataGenerator
    {
        private readonly Random random = new Random();

        public List<Data> GenerateEMGSignal(int numSamples, float duration, float muscleActivationLevel = 1f, float noiseAmplitude = 1.5f)
        {
            List<Data> emgSignal = new List<Data>();

            float timeIncrement = duration / numSamples;
            float currentTime = 0f;

            for (int i = 0; i < numSamples; i++)
            {
                float emgValue = GenerateEMGSample(currentTime, muscleActivationLevel) + GenerateNoise(noiseAmplitude);
                Data Point = new Data(i, emgValue);
                emgSignal.Add(Point);

                currentTime += timeIncrement;
            }

            return emgSignal;
        }

        private float GenerateEMGSample(float time, float muscleActivationLevel)
        {
            // Simulate the response with a combination of sine waves
            float frequency1 = 5f;  // Low-frequency component
            float frequency2 = 50f; // High-frequency component

            float emgValue = muscleActivationLevel * (
                2.0f * MathF.Sin(2 * MathF.PI * frequency1 * time) +
                1.0f * MathF.Sin(2 * MathF.PI * frequency2 * time)
            );

            return emgValue;
        }

        private float GenerateNoise(float amplitude)
        {
            // Generate random noise within the specified amplitude range
            return (float)(random.NextDouble() * 2 - 1) * amplitude;
        }
    }
}
