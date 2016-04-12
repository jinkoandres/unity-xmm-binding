//typedef unsigned long int std::size_t;
//typedef unsigned long int size_t;

//#include "xmm.h"		// xmm library's unique header
#include "XMMEngine.h"	// our bridge shared library's header
#include "XMMEngineCore.h"
#include <string>

// static variables :

//typedef XMMEngineCore xmm;
static XMMEngineCore x;
static float *likelihoods;
static std::string likeliest;
static std::string sp;
static std::string sm;
static std::string sts;
static const char *lastPhrase;

//================================================================//

void addPhrase(const char *label, const char **colNames, int nCols, float *phrase, int phraseSize) {
	// build phrase from data and add it to training set :
	// std::string sp;
	sp = "";
	sp += "{";
	sp += "\"bimodal\": false, ";
	sp += "\"dimension\": ";
	sp += std::to_string(nCols) + ", ";
	sp += "\"dimension_input\": 0, ";
	//====== column names ======//
	sp += "\"column_names\": [";
	for(int i=0; i<nCols-1; i++) {
		sp += "\"";
		sp += std::string(colNames[i]);
		sp += "\", ";
	}
	sp += "\"";
	sp += std::string(colNames[nCols-1]);
	sp += "\"], ";

	//========== data ==========//
	sp += "\"data\": [";
	for(int i=0; i<phraseSize-1; i++) {
		sp += std::to_string(*(phrase+i));
		sp += ", ";
	}
	sp += std::to_string(*(phrase+phraseSize-1));
	sp += "], ";

	sp += "\"length\": ";
	sp += std::to_string(static_cast<int>(phraseSize / nCols)) + ", ";
	sp += "\"label\": \"";
	sp += label;
	// add extra fields here (e.g. ID, date etc)
	sp += "\"}";

	lastPhrase = sp.c_str();
	x.addPhrase(sp);
}

const char *getLastPhrase() {
	return lastPhrase;
}

int getSetSize() {
	return x.getSetSize();
}

/*
const char **getSetLabels() {
	const char *[] = 
}
//*/

//void trainModels(const char **labels, int nLabels, const char **colNames, int nCols, int nGaussians) {
void train(int nbOfGaussians) {
	x.setNbOfGaussians(nbOfGaussians);
	x.train();
}

int getNbOfModels() {
	return x.getNbOfModels();
}

const char *getModels() {
	sm = x.getModels();
	return sm.c_str();
}

void setModels(const char *m) {
	sm = std::string(m);
	x.setModels(sm);
}

void clearModels() {
	x.clearModels();
}

const char *getTrainingSet() {
	sts = x.getTrainingSet();
	return sts.c_str();
}

void setTrainingSet(const char *ts) {
	sts = std::string(ts);
	x.setTrainingSet(sts);
}

void clearTrainingSet() {
	x.clearTrainingSet();
}

void clearLabel(const char *label) {
	std::string l(label);
	x.clearLabel(l);
}

// etc...

void setLikelihoodWindow(int w) {
	x.setLikelihoodWindow(w);
}

void filter(float *observation, int observationSize) {
	std::vector<float> obs;
	for(int i=0; i<observationSize; i++) {
		obs.push_back(observation[i]);
	}
	x.filter(obs);
}

const char *getLikeliest() {
	likeliest = x.getLikeliest();
	if(likeliest.compare("") == 0) {
		likeliest = "Unknown";
	}
	return likeliest.c_str();
}

float *getLikelihoods(int nLabels) {
	delete[] likelihoods;
	likelihoods = new float[nLabels];
	std::vector<double> vecLikelihoods = x.getLikelihoods();
	if(nLabels != vecLikelihoods.size()) {
		for(int i=0; i<nLabels; i++) {
			likelihoods[i] = -1;
		}
	} else {
		for(int i=0; i<nLabels; i++) {
			likelihoods[i] = vecLikelihoods[i];
		}
	}
	return likelihoods;
}
