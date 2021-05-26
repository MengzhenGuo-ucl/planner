//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Controller : MonoBehaviour
//{


//    VoxelGrid _voxelGrid;

//    bool _showLightAnalysis;
//    bool _getPlot = true;

//    float _tempDisplacement = 10f;


//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    //UI
//    private void OnGUI()
//    {
//        int UIHeight = 30;
//        int UIWidth = 150;
//        int i = 5;
//        int s = UIHeight - 5;

//        if (GUI.Button(new Rect(s,s*i++, UIWidth,UIHeight),"Modify the extension area"))
//        {
//            if (_getPlot == true)
//            {

//                _voxelGrid.SunlightAnalysis();
               
//            }

//            _showLightAnalysis = true;
//        }
//        _tempDisplacement = GUI.HorizontalSlider(new Rect(s, s * i++, UIWidth, UIHeight), _tempDisplacement, 0, 500);
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (_showLightAnalysis)
//        {
            
//        }
//    }
//}
