namespace VUtils {
    namespace MachineLearning {
        using System;
        public static class NeuralNetwork {
            public static float Forward(float[,] inputs, float bias) {
                float result = 0;
                for (int i = 0; i < inputs.Length; i++) {
                    result += inputs[i, 0] * inputs[i, 1];
                }
                result += bias;
                return result;
            }
            public static class ActivationFunctions {
                public static float Sigmoid(float input) { return 1 / (float)(1 + Math.Exp(input)); }
                public static float ReLU(float input) { return Math.Max(input, 0); }
            }

        }
    }
}