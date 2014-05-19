/****************************************************************************
*
* NAME: PitchShift.cs
* VERSION: 1.0
* HOME URL: http://www.dspdimension.com
* KNOWN BUGS: none
*
* SYNOPSIS: Routine for doing pitch shifting while maintaining
* duration using the Short Time Fourier Transform.
*
* DESCRIPTION: The routine takes a pitchShift factor value which is between 0.5
* (one octave down) and 2. (one octave up). A value of exactly 1 does not change
* the pitch. numSampsToProcess tells the routine how many samples in indata[0...
* numSampsToProcess-1] should be pitch shifted and moved to outdata[0 ...
* numSampsToProcess-1]. The two buffers can be identical (ie. it can process the
* data in-place). fftFrameSize defines the FFT frame size used for the
* processing. Typical values are 1024, 2048 and 4096. It may be any value <=
* MAX_FRAME_LENGTH but it MUST be a power of 2. osamp is the STFT
* oversampling factor which also determines the overlap between adjacent STFT
* frames. It should at least be 4 for moderate scaling ratios. A value of 32 is
* recommended for best quality. sampleRate takes the sample rate for the signal 
* in unit Hz, ie. 44100 for 44.1 kHz audio. The data passed to the routine in 
* indata[] should be in the range [-1.0, 1.0), which is also the output range 
* for the data, make sure you scale the data accordingly (for 16bit signed integers
* you would have to divide (and multiply) by 32768). 
*
* COPYRIGHT 1999-2006 Stephan M. Bernsee <smb [AT] dspdimension [DOT] com>
*
* 						The Wide Open License (WOL)
*
* Permission to use, copy, modify, distribute and sell this software and its
* documentation for any purpose is hereby granted without fee, provided that
* the above copyright notice and this license appear in all source copies. 
* THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT EXPRESS OR IMPLIED WARRANTY OF
* ANY KIND. See http://www.dspguru.com/wol.htm for more information.
*
*****************************************************************************/

/****************************************************************************
*
* This code was converted to C# by Michael Knight
* madmik3 at gmail dot com. 
* http://sites.google.com/site/mikescoderama/
*
*****************************************************************************/

using System;
using UnityEngine;

namespace TVR.Utils {
	public class AudioFilters {
		#region Pitch Shifter 
		private const int SHIFTMONSTER = 3;
		private const int SHIFTMOSQUITO = 3;

		public void Mosquito(float[] indata, out float[] outdata) {
			Mosquito(indata, out outdata, SHIFTMOSQUITO);
		}

		private void Mosquito(float[] indata, out float[] outdata, int shiftMosquito) {
			outdata = new float[(indata.Length - Mathf.FloorToInt(indata.Length / shiftMosquito)) + 1];
			for(int i = 0, j = 1, count = 0; i < indata.Length; ++i, ++j) {
				if(j < shiftMosquito) {
					if(count < outdata.Length) {
						outdata[count] = indata[i];
						count++;
					}
				} else
					j = 0;
			}
		}

		public void Monster(float[] indata, out float[] outdata) {
			outdata = new float[(indata.Length + Mathf.CeilToInt(indata.Length / SHIFTMONSTER)) + 1];
			for(int i = 0, j = 1, count = 0; i < indata.Length; ++i, ++j, ++count) {
				if(count < outdata.Length) {
					outdata[count] = indata[i];
					if(j >= SHIFTMONSTER) {
						count++;
						float value = indata[i];
						if(i < indata.Length - 1) {
							value += indata[i + 1];
							value /= 2f;
						}
						outdata[count] = value;
						j = 0;
					}
				}
			}
		}
		#endregion

		#region Echo
		private const float DECAYRATIOECHO = 1f / 4f; //Echo decay per delay. (>0 - <1)
		private const float DELAYECHO = 0.4f; //Echo delay in seconds.
		private const float DRYMIXECHO = 1f; //Volume of original signal to pass to output.
		private const float WETMIXECHO = 0.8f; //Volume of echo signal to pass to output.
		private bool AddSamples = true;

		public void Echo(float[] indata, out float[] outdata) {
			Echo(indata, out outdata, DECAYRATIOECHO, DELAYECHO, DRYMIXECHO, WETMIXECHO);
		}

		public void Echo(float[] indata, out float[] outdata, float decayFactor, float delay, float dryMix, float wetMix) {
			float initVolume = 1f - decayFactor;
			int echos = Mathf.CeilToInt(1 / decayFactor) - 1;
			float maxValue = 0;
			int[] delays = new int[echos];
			int[] initDelay = new int[echos];
			float value = 0f;

			for(int i = 0; i < echos; ++i) {
				delays[i] = (int)(-1 * delay * Globals.OUTPUTRATEPERSECOND * (i + 1));
				initDelay[i] = delays[i];
			}

			if(AddSamples)
				outdata = new float[indata.Length + (int)(echos * delay * Globals.OUTPUTRATEPERSECOND)];
			else
				outdata = new float[indata.Length];

			for(int i = 0; i < outdata.Length; ++i) {
				value = 0;
				for(int j = 0; j < echos; ++j) {
					if(delays[j] >= 0 && i + initDelay[j] < indata.Length) {
						value += indata[i + initDelay[j]] * (initVolume - (decayFactor * j));
					}
					delays[j]++;
				}
				value *= wetMix;
				if(i < indata.Length)
					value += (indata[i] * dryMix);
				if(maxValue < Mathf.Abs(value))
					maxValue = Mathf.Abs(value);
				outdata[i] = value;
			}
			if(maxValue > 1) {
				for(int i = 0; i < outdata.Length; ++i) {
					outdata[i] /= maxValue;
				}
			}
		}
		#endregion

		#region Robot
		private const int ROBOTSAMPLES = 7;
		private const int ROBOTRATIO = 32768 >> 2;

		private const int SHIFTROBOT = 5;

		private const float DECAYRATIOROBOT = 0.6f;
		private const float DELAYROBOT = 0.04f;
		private const float DRYMIXROBOT = 1f;
		private const float WETMIXROBOT = 1f;
		private const int REVERBROBOT = 3;

		public void Robot(float[] indata, out float[] outdata) {
			Compression(indata, out outdata, ROBOTSAMPLES, ROBOTRATIO);
			Mosquito(outdata, out outdata, SHIFTROBOT);
			for(int i = 0; i < REVERBROBOT; ++i)
				Echo(outdata, out outdata, DECAYRATIOROBOT, DELAYROBOT, DRYMIXROBOT, WETMIXROBOT);
		}
		#endregion

		#region Distorsion
		private const float DISTROSIONQUANTITY = 0.035f; //(>0 - <1)
		public void Distorsion(float[] indata, out float[] outdata) {
			outdata = new float[indata.Length];
			for(int i = 0; i < indata.Length; ++i) {
				outdata[i] = Mathf.Round(indata[i] / DISTROSIONQUANTITY) * DISTROSIONQUANTITY;
			}
		}
		#endregion

		#region Noise
		private const float NOISEVOLUME = 0.05f;

		public void Noise(float[] indata, out float[] outdata) {
			float noiseVolume;
			float maxValue = 0;
			float value = 0f;
			System.Random r = new System.Random();
			float rnd;
			float normalize = 1;

			for(int i = 0; i < indata.Length; ++i) {
				if(maxValue < Mathf.Abs(indata[i]))
					maxValue = Mathf.Abs(indata[i]);
			}
			noiseVolume = NOISEVOLUME * maxValue;
			if(noiseVolume + maxValue > 1) {
				normalize += 1 - (noiseVolume + maxValue);
			}
			float noiseValue = noiseVolume * 2f;

			outdata = new float[indata.Length];
			for(int i = 0; i < outdata.Length; ++i) {
				rnd = (float)r.NextDouble();
				value = (indata[i] * normalize) + ((rnd * noiseValue) - noiseVolume);
				/*if(maxValue < Mathf.Abs(value))
					maxValue = Mathf.Abs(value);*/
				outdata[i] = value;
			}
			/*if(maxValue > 1) {
				for(int i = 0; i < outdata.Length; ++i) {
					outdata[i] /= maxValue;
				}
			}*/
		}
		#endregion

		#region Compression
		private const int COMPRESSIONSAMPLES = 2;
		private const int COMPRESSIONRATIO = 32768 >> 10;

		public void Compression(float[] indata, out float[] outdata) {
			Compression(indata, out outdata, COMPRESSIONSAMPLES, COMPRESSIONRATIO);
		}

		private void Compression(float[] indata, out float[] outdata, int compressionSamples, int compressionRatio) {
			outdata = new float[indata.Length];
			float value;
			for(int i = 0; i < indata.Length; i += compressionSamples) {
				value = 0;
				for(int j = 0; j < compressionSamples; j++) {
					if(i + j < indata.Length)
						value += indata[i + j];
				}
				value /= compressionSamples;

				if(compressionRatio != 32768)
					value = (int)(value * compressionRatio) / (float)compressionRatio;

				for(int j = 0; j < compressionSamples; j++) {
					if(i + j < outdata.Length)
						outdata[i + j] = value;
				}
			}
		}
		#endregion

		#region Pitch Shifter (Pro)
		const float SHIFTMONSTERPRO = 0.7f;
		const float SHIFTMOSQUITOPRO = 1.5f;
		private const int MAX_FRAME_LENGTH = 4096; //16000;
		private float[] gInFIFO = new float[MAX_FRAME_LENGTH];
		private float[] gOutFIFO = new float[MAX_FRAME_LENGTH];
		private float[] gFFTworksp = new float[2 * MAX_FRAME_LENGTH];
		private float[] gLastPhase = new float[MAX_FRAME_LENGTH / 2 + 1];
		private float[] gSumPhase = new float[MAX_FRAME_LENGTH / 2 + 1];
		private float[] gOutputAccum = new float[2 * MAX_FRAME_LENGTH];
		private float[] gAnaFreq = new float[MAX_FRAME_LENGTH];
		private float[] gAnaMagn = new float[MAX_FRAME_LENGTH];
		private float[] gSynFreq = new float[MAX_FRAME_LENGTH];
		private float[] gSynMagn = new float[MAX_FRAME_LENGTH];
		private long gRover;//, gInit;

		public void MosquitoPro(float[] indata, out float[] outdata) {
			PitchShift(SHIFTMOSQUITOPRO, indata.Length, Globals.OUTPUTRATEPERSECOND, indata, out outdata);
		}

		public void MonsterPro(float[] indata, out float[] outdata) {
			PitchShift(SHIFTMONSTERPRO, indata.Length, Globals.OUTPUTRATEPERSECOND, indata, out outdata);
		}

		public void PitchShift(float pitchShift, long numSampsToProcess, float sampleRate, float[] indata, out float[] outdata) {
			PitchShift(pitchShift, numSampsToProcess, (long)2048, (long)10, sampleRate, indata, out outdata);
		}

        public void PitchShift(float pitchShift, long numSampsToProcess, long fftFrameSize, long osamp, float sampleRate, float[] indata, out float[] outdata) {
			float magn, phase, tmp, window, real, imag;
			float freqPerBin, expct;
			long i, k, qpd, index, inFifoLatency, stepSize, fftFrameSize2;

			//float[] outdata = indata;
			outdata = indata;
			/* set up some handy variables */
			fftFrameSize2 = fftFrameSize / 2;
			stepSize = fftFrameSize / osamp;
			freqPerBin = sampleRate / (float)fftFrameSize;
			expct = 2.0f * Mathf.PI * (float)stepSize / (float)fftFrameSize;
			inFifoLatency = fftFrameSize - stepSize;
			/*if (gRover == 0)*/
			gRover = inFifoLatency;


			/* main processing loop */
			for(i = 0; i < numSampsToProcess; i++) {

				/* As long as we have not yet collected enough data just read in */
				gInFIFO[gRover] = indata[i];
				outdata[i] = gOutFIFO[gRover - inFifoLatency];
				gRover++;

				/* now we have enough data for processing */
				if(gRover >= fftFrameSize) {
					gRover = inFifoLatency;

					/* do windowing and re,im interleave */
					for(k = 0; k < fftFrameSize; k++) {
						window = -.5f * Mathf.Cos(2.0f * Mathf.PI * k / (float)fftFrameSize) + .5f;
						gFFTworksp[2 * k] = (float)(gInFIFO[k] * window);
						gFFTworksp[2 * k + 1] = 0.0F;
					}


					/* ***************** ANALYSIS ******************* */
					/* do transform */
					ShortTimeFourierTransform(gFFTworksp, fftFrameSize, -1);

					/* this is the analysis step */
					for(k = 0; k <= fftFrameSize2; k++) {

						/* de-interlace FFT buffer */
						real = gFFTworksp[2 * k];
						imag = gFFTworksp[2 * k + 1];

						/* compute magnitude and phase */
						magn = 2.0f * Mathf.Sqrt(real * real + imag * imag);
						phase = Mathf.Atan2(imag, real);

						/* compute phase difference */
						tmp = phase - gLastPhase[k];
						gLastPhase[k] = (float)phase;

						/* subtract expected phase difference */
						tmp -= (float)k * expct;

						/* map delta phase into +/- Pi interval */
						qpd = (long)(tmp / Mathf.PI);
						if(qpd >= 0)
							qpd += qpd & 1;
						else
							qpd -= qpd & 1;
						tmp -= Mathf.PI * (float)qpd;

						/* get deviation from bin frequency from the +/- Pi interval */
						tmp = osamp * tmp / (2.0f * Mathf.PI);

						/* compute the k-th partials' true frequency */
						tmp = (float)k * freqPerBin + tmp * freqPerBin;

						/* store magnitude and true frequency in analysis arrays */
						gAnaMagn[k] = (float)magn;
						gAnaFreq[k] = (float)tmp;

					}

					/* ***************** PROCESSING ******************* */
					/* this does the actual pitch shifting */
					for(int zero = 0; zero < fftFrameSize; zero++) {
						gSynMagn[zero] = 0;
						gSynFreq[zero] = 0;
					}

					for(k = 0; k <= fftFrameSize2; k++) {
						index = (long)(k * pitchShift);
						if(index <= fftFrameSize2) {
							gSynMagn[index] += gAnaMagn[k];
							gSynFreq[index] = gAnaFreq[k] * pitchShift;
						}
					}

					/* ***************** SYNTHESIS ******************* */
					/* this is the synthesis step */
					for(k = 0; k <= fftFrameSize2; k++) {

						/* get magnitude and true frequency from synthesis arrays */
						magn = gSynMagn[k];
						tmp = gSynFreq[k];

						/* subtract bin mid frequency */
						tmp -= (float)k * freqPerBin;

						/* get bin deviation from freq deviation */
						tmp /= freqPerBin;

						/* take osamp into account */
						tmp = 2.0f * Mathf.PI * tmp / osamp;

						/* add the overlap phase advance back in */
						tmp += (float)k * expct;

						/* accumulate delta phase to get bin phase */
						gSumPhase[k] += (float)tmp;
						phase = gSumPhase[k];

						/* get real and imag part and re-interleave */
						gFFTworksp[2 * k] = (float)(magn * Mathf.Cos(phase));
						gFFTworksp[2 * k + 1] = (float)(magn * Mathf.Sin(phase));
					}

					/* zero negative frequencies */
					for(k = fftFrameSize + 2; k < 2 * fftFrameSize; k++)
						gFFTworksp[k] = 0.0F;

					/* do inverse transform */
					ShortTimeFourierTransform(gFFTworksp, fftFrameSize, 1);

					/* do windowing and add to output accumulator */
					for(k = 0; k < fftFrameSize; k++) {
						window = -.5f * Mathf.Cos(2.0f * Mathf.PI * (float)k / (float)fftFrameSize) + .5f;
						gOutputAccum[k] += 2.0f * window * gFFTworksp[2 * k] / (fftFrameSize2 * osamp);
					}
					for(k = 0; k < stepSize; k++)
						gOutFIFO[k] = gOutputAccum[k];

					/* shift accumulator */
					//memmove(gOutputAccum, gOutputAccum + stepSize, fftFrameSize * sizeof(float));
					for(k = 0; k < fftFrameSize; k++) {
						gOutputAccum[k] = gOutputAccum[k + stepSize];
					}

					/* move input FIFO */
					for(k = 0; k < inFifoLatency; k++)
						gInFIFO[k] = gInFIFO[k + stepSize];
				}
			}
			/*gInFIFO = null;
			gOutFIFO = null;
			gFFTworksp = null;
			gLastPhase = null;
			gSumPhase = null;
			gOutputAccum = null;
			gAnaFreq = null;
			gAnaMagn = null;
			gSynFreq = null;
			gSynMagn = null;*/
		}

		void ShortTimeFourierTransform(float[] fftBuffer, long fftFrameSize, long sign) {
			float wr, wi, arg, temp;
			float tr, ti, ur, ui;
			long i, bitm, j, le, le2, k;

			for(i = 2; i < 2 * fftFrameSize - 2; i += 2) {
				for(bitm = 2, j = 0; bitm < 2 * fftFrameSize; bitm <<= 1) {
					if((i & bitm) != 0)
						j++;
					j <<= 1;
				}
				if(i < j) {
					temp = fftBuffer[i];
					fftBuffer[i] = fftBuffer[j];
					fftBuffer[j] = temp;
					temp = fftBuffer[i + 1];
					fftBuffer[i + 1] = fftBuffer[j + 1];
					fftBuffer[j + 1] = temp;
				}
			}
			long max = (long)(Mathf.Log(fftFrameSize) / Mathf.Log(2.0f) + .5f);
			for(k = 0, le = 2; k < max; k++) {
				le <<= 1;
				le2 = le >> 1;
				ur = 1.0F;
				ui = 0.0F;
				arg = (float)Mathf.PI / (le2 >> 1);
				wr = (float)Mathf.Cos(arg);
				wi = (float)(sign * Mathf.Sin(arg));
				for(j = 0; j < le2; j += 2) {

					for(i = j; i < 2 * fftFrameSize; i += le) {
						tr = fftBuffer[i + le2] * ur - fftBuffer[i + le2 + 1] * ui;
						ti = fftBuffer[i + le2] * ui + fftBuffer[i + le2 + 1] * ur;
						fftBuffer[i + le2] = fftBuffer[i] - tr;
						fftBuffer[i + le2 + 1] = fftBuffer[i + 1] - ti;
						fftBuffer[i] += tr;
						fftBuffer[i + 1] += ti;

					}
					tr = ur * wr - ui * wi;
					ui = ur * wi + ui * wr;
					ur = tr;
				}
			}
		}
        #endregion
	}
}