using System;
using Unity.Entities;
namespace VUtils {
    namespace MachineLearning {
        public static class NeuralNetwork {
            public static float Forward(float[,] inputs, float bias) {
                float result = 0;
                for (int i = 0; i < inputs.Length; i++) {
                    result += inputs[i, 0] * inputs[i, 1];
                }
                result += bias;
                return result;
            }
            private static float Forward(float input, float bias, float weight) {
                return input * bias + weight;
            }
            public static void Forward(DynamicBuffer<Interaction> inters) {
                for (int i = 0; i < inters.Length; i++) {
                    var output = Forward(inters[i].Input.Value, inters[i].Output.Bias, inters[i].Weight);
                    inters[i] = new Interaction {
                        ID = inters[i].ID,
                        Input = inters[i].Input,
                        Output = new Node {
                            ID = inters[i].Output.ID,
                            Bias = inters[i].Output.Value,
                            Type = inters[i].Output.Type,
                            Value = output
                        }
                    };
                }
            }
            public static class ActivationFunctions {
                public static float Sigmoid(float input) { return 1 / (float)(1 + Math.Exp(input)); }
                public static float ReLU(float input) { return Math.Max(input, 0); }
            }

        }
    }
}