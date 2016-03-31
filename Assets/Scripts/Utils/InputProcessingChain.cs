using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputProcessingChain {

	int windowSize;
	int hopSize;
	int cnt;
	bool newFrame;
	List<float> buffer;
	List<float> frame;
	float[] outFrame;

	float noiseThreshold;
	float mean;
	float magnitude;
	float stdDev;
	List<float> crossings;
	float periodMean;
	float periodStdDev;
	
	public InputProcessingChain(int wSize, int hSize) {
		windowSize = wSize;
		hopSize = hSize;
		cnt = 0;
		buffer = new List<float>();
		for(int i=0; i<windowSize; i++) {
			buffer.Add(0f);
		}
		outFrame = new float[3] {0f, 0f, 0f};

		noiseThreshold = 0.05f;
		mean = 0;
		magnitude = 0;
		stdDev = 0;
		crossings = new List<float>();
		periodMean = 0;
		periodStdDev = 0;
	}

	// Use this for initialization
	public void feed (float newSample) {
		buffer.Add(newSample);
		cnt++;

		if(cnt == hopSize) {// && buffer.Count > windowSize - 1) {
			newFrame = true;
			frame = buffer.GetRange(buffer.Count - windowSize - 1, windowSize);
			processFrame();

			buffer = frame;
			cnt = 0;
		}
	}

	public int getBufferSize() {
		return buffer.Count;
	}
	
	public bool hasNewFrame () {
		if(newFrame) {
			return true;
		}
		return false;
	}

	public float[] getLastFrame() {
		if(newFrame) {
			newFrame = false;
		}
		return outFrame;
	}

	void processFrame() {

		//=================== FIRST COMPUTE MEAN ===============//
		float min, max;
		min = max = frame[0];
		mean = 0;
		magnitude = 0;

		for(int i=0; i<frame.Count; i++) {
			float val = frame[i];
			magnitude += val * val;
			mean += val;
			if(val > max) {
				max = val;
			} else if(val < min) {
				min = val;
			}
		}
		// TODO : more tests to determine which mean (true mean or (max-min)/2) is the best
		//mean /= frame.Count;
		mean = (max - min) * 0.5f;

		magnitude /= frame.Count;
		magnitude = Mathf.Sqrt(magnitude);

		//================= STDDEV + MEAN-XINGS ===============//
		crossings = new List<float>();
		stdDev = 0;
		float prevDelta = frame[0] - mean;
		for(int i=0; i<frame.Count; i++) {
			float delta = frame[i] - mean;
			stdDev += delta * delta;
			if(prevDelta > noiseThreshold && delta < noiseThreshold) {
				crossings.Add(i);
			}
			prevDelta = delta;
		}
		stdDev /= (frame.Count - 1);
		stdDev = Mathf.Sqrt(stdDev);

		//============ MEAN OF DELTA-T BETWEEN XINGS ==========//
		periodMean = 0;
		for(int i=1; i<crossings.Count; i++) {
			periodMean += crossings[i] - crossings[i - 1];
		}
		periodMean /= (crossings.Count - 1);

		//=========== STDDEV OF DELTA-T BETWEEN XINGS =========//
		periodStdDev = 0;
		for(int i=1; i<crossings.Count; i++) {
			float deltaP = (crossings[i] - crossings[i - 1] - periodMean);
			periodStdDev += deltaP * deltaP;
		}
		if(crossings.Count > 2) {
			periodStdDev = Mathf.Sqrt(periodStdDev / (crossings.Count - 2));
		}

		//=================== FILL OUTFRAME ===================//
		outFrame[0] = stdDev * 2;
		outFrame[1] = 5f * crossings.Count / frame.Count;
		outFrame[2] = periodStdDev / frame.Count;
	}
}
