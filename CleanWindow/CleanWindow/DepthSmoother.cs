using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Kinect;

namespace CleanWindow
{
    public static class DepthSmoother
    {
        public static bool UseFiltering = true;
        // Will specify how many non-zero pixels within a 1 pixel band
        // around the origin there should be before a filter is applied
        private static int InnerBandThreshold = 2;
        // Will specify how many non-zero pixels within a 2 pixel band
        // around the origin there should be before a filter is applied
        private static int OuterBandThreshold = 2;

        // Constants used to map value ranges for distance to pixel intensity conversions
        private const int MaxDepthDistance = 4000;
        private const int MinDepthDistance = 1000;
        private const int MaxDepthDistanceOffset = MaxDepthDistance - MinDepthDistance;

        private static Queue<short[]> averageQueue = new Queue<short[]>();
        private static int averageFrameCount = 1;

        static DepthSmoother()
        {
        }

        public static short[] CreateSmoothImageFromDepthArray(DepthImageFrame image)
        {
            int width = image.Width;
            int height = image.Height;

            short[] depthArray = new short[image.PixelDataLength];

            image.CopyPixelDataTo(depthArray);

            return CreateFilteredDepthArray(depthArray, width, height);

        }

        public static short[] CreateFilteredDepthArray(short[] depthArray, int width, int height)
        {
            /////////////////////////////////////////////////////////////////////////////////////
            // I will try to comment this as well as I can in here, but you should probably refer
            // to my Code Project article for a more in depth description of the method.
            /////////////////////////////////////////////////////////////////////////////////////

            short[] smoothDepthArray = new short[depthArray.Length];

            // We will be using these numbers for constraints on indexes
            int widthBound = width - 1;
            int heightBound = height - 1;

            // We process each row in parallel
            //for (int depthArrayRowIndex = 0; depthArrayRowIndex < 480; depthArrayRowIndex++)
            Parallel.For(0, 480, depthArrayRowIndex =>
            {
                // Process each pixel in the row
                for (int depthArrayColumnIndex = 0; depthArrayColumnIndex < 640; depthArrayColumnIndex++)
                {
                    var depthIndex = depthArrayColumnIndex + (depthArrayRowIndex * 640);

                    // We are only concerned with eliminating 'white' noise from the data.
                    // We consider any pixel with a depth of 0 as a possible candidate for filtering.
                    if (depthArray[depthIndex] == 0)
                    {
                        // From the depth index, we can determine the X and Y coordinates that the index
                        // will appear in the image.  We use this to help us define our filter matrix.
                        int x = depthIndex % 640;
                        int y = (depthIndex - x) / 640;

                        // The filter collection is used to count the frequency of each
                        // depth value in the filter array.  This is used later to determine
                        // the statistical mode for possible assignment to the candidate.
                        short[,] filterCollection = new short[24, 2];

                        // The inner and outer band counts are used later to compare against the threshold 
                        // values set in the UI to identify a positive filter result.
                        int innerBandCount = 0;
                        int outerBandCount = 0;

                        // The following loops will loop through a 5 X 5 matrix of pixels surrounding the 
                        // candidate pixel.  This defines 2 distinct 'bands' around the candidate pixel.
                        // If any of the pixels in this matrix are non-0, we will accumulate them and count
                        // how many non-0 pixels are in each band.  If the number of non-0 pixels breaks the
                        // threshold in either band, then the average of all non-0 pixels in the matrix is applied
                        // to the candidate pixel.
                        for (int yi = -2; yi < 3; yi++)
                        {
                            for (int xi = -2; xi < 3; xi++)
                            {
                                // yi and xi are modifiers that will be subtracted from and added to the
                                // candidate pixel's x and y coordinates that we calculated earlier.  From the
                                // resulting coordinates, we can calculate the index to be addressed for processing.

                                // We do not want to consider the candidate pixel (xi = 0, yi = 0) in our process at this point.
                                // We already know that it's 0
                                if (xi != 0 || yi != 0)
                                {
                                    // We then create our modified coordinates for each pass
                                    var xSearch = x + xi;
                                    var ySearch = y + yi;

                                    // While the modified coordinates may in fact calculate out to an actual index, it 
                                    // might not be the one we want.  Be sure to check to make sure that the modified coordinates
                                    // match up with our image bounds.
                                    if (xSearch >= 0 && xSearch <= widthBound && ySearch >= 0 && ySearch <= heightBound)
                                    {
                                        var index = xSearch + (ySearch * width);
                                        // We only want to look for non-0 values
                                        if (depthArray[index] != 0)
                                        {
                                            // We want to find count the frequency of each depth
                                            for (int i = 0; i < 24; i++)
                                            {
                                                if (filterCollection[i, 0] == depthArray[index])
                                                {
                                                    // When the depth is already in the filter collection
                                                    // we will just increment the frequency.
                                                    filterCollection[i, 1]++;
                                                    break;
                                                }
                                                else if (filterCollection[i, 0] == 0)
                                                {
                                                    // When we encounter a 0 depth in the filter collection
                                                    // this means we have reached the end of values already counted.
                                                    // We will then add the new depth and start it's frequency at 1.
                                                    filterCollection[i, 0] = depthArray[index];
                                                    filterCollection[i, 1]++;
                                                    break;
                                                }
                                            }

                                            // We will then determine which band the non-0 pixel
                                            // was found in, and increment the band counters.
                                            if (yi != 2 && yi != -2 && xi != 2 && xi != -2)
                                                innerBandCount++;
                                            else
                                                outerBandCount++;
                                        }
                                    }
                                }
                            }
                        }

                        // Once we have determined our inner and outer band non-zero counts, and accumulated all of those values,
                        // we can compare it against the threshold to determine if our candidate pixel will be changed to the
                        // statistical mode of the non-zero surrounding pixels.
                        if (innerBandCount >= InnerBandThreshold || outerBandCount >= OuterBandThreshold)
                        {
                            short frequency = 0;
                            short depth = 0;
                            // This loop will determine the statistical mode
                            // of the surrounding pixels for assignment to
                            // the candidate.
                            for (int i = 0; i < 24; i++)
                            {
                                // This means we have reached the end of our
                                // frequency distribution and can break out of the
                                // loop to save time.
                                if (filterCollection[i, 0] == 0)
                                    break;
                                if (filterCollection[i, 1] > frequency)
                                {
                                    depth = filterCollection[i, 0];
                                    frequency = filterCollection[i, 1];
                                }
                            }

                            smoothDepthArray[depthIndex] = depth;
                        }

                    }
                    else
                    {
                        // If the pixel is not zero, we will keep the original depth.
                        smoothDepthArray[depthIndex] = depthArray[depthIndex];
                    }
                }
            });

            return smoothDepthArray;
        }


        public static short[] CreateAverageDepthArray(short[] depthArray)
        {
            // This is a method of Weighted Moving Average per pixel coordinate across several frames of depth data.
            // This means that newer frames are linearly weighted heavier than older frames to reduce motion tails,
            // while still having the effect of reducing noise flickering.

            averageQueue.Enqueue(depthArray);

            CheckForDequeue();

            int[] sumDepthArray = new int[depthArray.Length];
            short[] averagedDepthArray = new short[depthArray.Length];

            int Denominator = 0;
            int Count = 1;

            // REMEMBER!!! Queue's are FIFO (first in, first out).  This means that when you iterate
            // over them, you will encounter the oldest frame first.

            // We first create a single array, summing all of the pixels of each frame on a weighted basis
            // and determining the denominator that we will be using later.
            foreach (var item in averageQueue)
            {
                // Process each row in parallel
                Parallel.For(0, 480, depthArrayRowIndex =>
                {
                    // Process each pixel in the row
                    for (int depthArrayColumnIndex = 0; depthArrayColumnIndex < 640; depthArrayColumnIndex++)
                    {
                        var index = depthArrayColumnIndex + (depthArrayRowIndex * 640);
                        sumDepthArray[index] += item[index] * Count;
                    }
                });
                Denominator += Count;
                Count++;
            }

            // Once we have summed all of the information on a weighted basis, we can divide each pixel
            // by our calculated denominator to get a weighted average.

            // Process each row in parallel
            Parallel.For(0, 480, depthArrayRowIndex =>
            {
                // Process each pixel in the row
                for (int depthArrayColumnIndex = 0; depthArrayColumnIndex < 640; depthArrayColumnIndex++)
                {
                    var index = depthArrayColumnIndex + (depthArrayRowIndex * 640);
                    averagedDepthArray[index] = (short)(sumDepthArray[index] / Denominator);
                }
            });

            return averagedDepthArray;
        }

        private static void CheckForDequeue()
        {
            // We will recursively check to make sure we have Dequeued enough frames.
            // This is due to the fact that a user could constantly be changing the UI element
            // that specifies how many frames to use for averaging.
            if (averageQueue.Count > averageFrameCount)
            {
                averageQueue.Dequeue();
                CheckForDequeue();
            }
        }
    }

}