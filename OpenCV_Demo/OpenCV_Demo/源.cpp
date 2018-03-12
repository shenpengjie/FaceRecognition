#include <iostream> 
#include<string>
#include <opencv2/opencv.hpp>
#include<opencv2/highgui.hpp>
#include<opencv2/core/core.hpp>
#include<opencv2/imgproc/imgproc.hpp>

using namespace std;
using namespace cv;
string change(int n)
{
	string res ;
	while (n>0)
	{
		res += n % 10 + 48;
		n /= 10;
	}
	string res2;
	for (int i = res.size()-1; i >=0; i--)
	{
		res2 += res[i];
	}
	return res2;
}
int main()
{
	string str;
	VideoCapture cap(0);
	if (!cap.isOpened())
	{
		return -1;
	}
	Mat frame;
	String m = "E:\\";
	
	for (int i = 0; i < 1000; i++)
	{
		cap >> frame;
		imshow("soj", frame);
		char key = waitKey(30);
		if (key == 27)
		{
			break;
		}
		str=change(i);
		string path = m + str + ".jpg";
		imwrite(path, frame);

	}

}