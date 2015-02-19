/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class BarPresentation : MonoBehaviour {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    public int currentSlideNum = 1;
    public GameObject slidesBar;
    
    public Slide[] slides;
    private Slide currentSlide;
    
    private EnergyBar bar;
    
    // ===========================================================
    // Constructors (Including Static Constructors)
    // ===========================================================

    // ===========================================================
    // Getters / Setters
    // ===========================================================

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================

    void Start() {
        bar = slidesBar.GetComponent<EnergyBar>();
        bar.valueMax = slides.Length;
        
        HideAll();
    }
    
    void Update() {
        if (slides.Length == 0) {
            return;
        } else if (currentSlide == null) {
            ChangeSlide(currentSlideNum);
        }
        
        currentSlideNum = Mathf.Clamp(currentSlideNum, 1, slides.Length);
        
        bar.valueCurrent = currentSlideNum;
    }

    void OnGUI() {
        if (slides.Length == 0) {
            return;
        }
        
        //
        // draw default controls
        //
        if (currentSlideNum != 1) {
            if (GUI.Button(new Rect(140, 15, 80, 30), "<< Prev")) {
                PreviousSlide();
            }
        }
        
        if (currentSlideNum != slides.Length) {
            if (GUI.Button(new Rect(580, 15, 80, 30), "Next >>")) {
                NextSlide();
            }
        }
        
    
        if (currentSlideNum > slides.Length) {
            return;
        }
    }
    

    // ===========================================================
    // Methods
    // ===========================================================
    
    private void HideAll() {
        foreach (Slide slide in slides) {
            slide.Hide();
        }
    }
    
    private void NextSlide() {
        ChangeSlide(currentSlideNum + 1);
    }
    
    private void PreviousSlide() {
        ChangeSlide(currentSlideNum - 1);
    }
    
    private void ChangeSlide(int num) {
        if (currentSlide != null) {
            currentSlide.Hide();
        }
        
        currentSlide = slides[num - 1];
        currentSlide.Show();
        
        currentSlideNum = num;
    }
    
    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================
    
    [System.Serializable]
    public class Slide {
        public GameObject gameObject;
        
        public void Show() {
            gameObject.SetActive(true);
        }
        
        public void Hide() {
            gameObject.SetActive(false);
        }
    }

}