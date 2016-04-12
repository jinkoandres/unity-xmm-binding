#include "XMMEngineCore.h"

void
XMMEngineCore::setNbOfGaussians(int n) {
	gmm.configuration.gaussians.set(n, 1);
	gmm.configuration.changed = true;
}

void
XMMEngineCore::train() {
	gmm.train(&set);
	gmm.reset(); // reset for next call to filter
}

void
XMMEngineCore::setLikelihoodWindow(int w) {
	gmm.shared_parameters->likelihood_window.set(w);
}

void
XMMEngineCore::filter(std::vector<float> observation) {
	gmm.filter(observation);
}

std::vector<double>
XMMEngineCore::getLikelihoods() {
	return gmm.results.smoothed_normalized_likelihoods;
}

std::string
XMMEngineCore::getLikeliest() {
	return gmm.results.likeliest;
}

int
XMMEngineCore::getNbOfModels() {
	return gmm.models.size();
}

std::string
XMMEngineCore::getModels() {
	Json::Value jm = gmm.toJson();
	Json::FastWriter fw;
	return fw.write(jm);
}

void
XMMEngineCore::setModels(std::string sm) {
	Json::Value jm;
	Json::Reader jr;
	if(jr.parse(sm, jm)) {
		// TODO : uncomment when implementation repaired
		//gmm.fromJson(jm);
	} else {
		throw std::runtime_error("Cannot Parse Json String");
	}
}

void
XMMEngineCore::clearModels() {
	gmm.clear();
}

std::string
XMMEngineCore::getTrainingSet() {
	Json::Value jts = set.toJson();
	Json::FastWriter fw;
	return fw.write(jts);
}

void
XMMEngineCore::setTrainingSet(std::string sts) {
	Json::Value jts;
	Json::Reader jr;
	if(jr.parse(sts, jts)) {
		set.fromJson(jts);
	} else {
		throw std::runtime_error("Cannot Parse Json String");
	}
}

void
XMMEngineCore::clearTrainingSet() {
	set.clear();
}

void
XMMEngineCore::clearLabel(std::string label) {
	xmm::TrainingSet *s = set.getPhrasesOfClass(label);
	for(auto const &phrase : *s) {
		freeList.push_back(phrase.first);
	}
	std::sort(freeList.begin(), freeList.end(), std::greater<int>());
	set.removePhrasesOfClass(label);
}

void
XMMEngineCore::addPhrase(std::string sp) {
	xmm::Phrase xp;
	Json::Value jp;
	Json::Reader jr;
	if(jr.parse(sp, jp)) {
		xp.fromJson(jp);
		int index;
		if(freeList.size() == 0) {
			index = set.size();
		} else {
			index = freeList.back();
			freeList.pop_back();
		}
		set.addPhrase(index, xp);
		set.dimension.set(xp.dimension.get());
		set.dimension_input.set(0);
		set.column_names.set(xp.column_names, true);
	} else {
		throw std::runtime_error("Cannot Parse Json String");
	}
}

int
XMMEngineCore::getSetSize() {
	return static_cast<int>(set.size());
}

