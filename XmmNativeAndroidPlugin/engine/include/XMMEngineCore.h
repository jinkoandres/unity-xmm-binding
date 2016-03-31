#include "xmm.h"

class XMMEngineCore {

private:
	xmm::TrainingSet set;
	xmm::GMM gmm;
	//xmm::HierarchicalHMM hhmm;

public:
	XMMEngineCore() {};
	~XMMEngineCore() {};

	void addPhrase(std::string sp);
	int getSetSize();
	void clearSet();
	void clearLabel(std::string label);
	void setNbOfGaussians(int n);
	void train();

	void setLikelihoodWindow(int w);
	void filter(std::vector<float> observation);
	std::vector<double> getLikelihoods();
	std::string getLikeliest();

	int getNbOfModels();
	std::string getModel();
	void setModel(std::string sm);
};