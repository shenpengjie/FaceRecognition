import com.baidu.aip.face.AipFace;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;

public class Sample {

    public static final String APP_ID = "10393744";
    public static final String API_KEY = "5h0lo8Vfn2pjUt8yGU7GDw53";
    public static final String SECRET_KEY = "MxknT7KKNBbgGr8WH01aAiOgsMN4WAGY";
    public static void main(String path) {

        AipFace client = new AipFace(APP_ID, API_KEY, SECRET_KEY);


        client.setConnectionTimeoutInMillis(2000);
        client.setSocketTimeoutInMillis(60000);


        String imagePath1 = "C:\\Users\\spj\\Desktop\\test.jpg";
        String imagePath2 = path;
        ArrayList<String> pathArray = new ArrayList<String>();
        pathArray.add(imagePath1);
        pathArray.add(imagePath2);
        JSONObject response = client.match(pathArray, new HashMap<String, String>());
        String result=response.toString();
        //System.out.println(result);
        String str="";
        boolean judge=false;
        if(result!=null&&!"".equals(result)) {
        for (int i=0;i<result.length();i++) {
            if(result.charAt(i)>=48&&result.charAt(i)<=57||result.charAt(i)=='.'){
                str+=result.charAt(i);
                judge=true;
            }
            else if(judge==true){
                break;
            }
        }
        }
        Double v=Double.valueOf(str);
        if(v>100) System.out.println("图片对比错误");
        else
            System.out.println("两张照片的相似度为："+str+"%");
        System.out.println(response.toString(2));
    }

}