using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CleanWindow
{
    public class ConvolutionMatrix
    {
        public int MatrixSize = 3;

        public double[,] Matrix;
        public double Factor = 1;
        public double Offset = 1;

        public ConvolutionMatrix(int size)
        {
            MatrixSize = 3;
            Matrix = new double[size, size];
        }

        public void SetAll(double value)
        {
            for (int i = 0; i < MatrixSize; i++)
            {
                for (int j = 0; j < MatrixSize; j++)
                {
                    Matrix[i, j] = value;
                }
            }
        }
    }

}
