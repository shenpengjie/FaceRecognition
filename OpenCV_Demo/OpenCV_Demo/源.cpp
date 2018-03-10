#include <iostream>  
#include <opencv2/opencv.hpp>
#include<opencv2/highgui.hpp>
#include<opencv2/core/core.hpp>
#include<opencv2/imgproc/imgproc.hpp>

using namespace std;
using namespace cv;

int main()
{
	Mat src = imread("Path");//  ‰»Î¬∑æ∂
	Mat img = Mat::zeros(src.size(), CV_8UC1);
	cvtColor(src, src, CV_RGB2GRAY);
	threshold(src, src, 160, 250, THRESH_BINARY);
	vector<vector<Point>>contours;
	//vector<Vec4i> hierrarchy;
	findContours(src, contours, 1, CHAIN_APPROX_SIMPLE);
	Mat result = Mat::zeros(src.size(), CV_8UC1);
	if (!contours.empty())
	{
		drawContours(result, contours, -1, Scalar(255), 2);
		imshow("spj", result);
		waitKey(0);
	}
	
}