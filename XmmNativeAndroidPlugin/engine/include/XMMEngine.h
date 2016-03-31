extern "C" {

	void addPhrase(const char *label, const char **colNames, int nCols, float *phrase, int phraseSize);

	const char *getLastPhrase();

	int getSetSize();

	//const char **getSetLabels();

	void clearSet();

	void clearLabel(const char *label);

	void train(int nbOfGaussians);

	int getNbOfModels();

	const char *getModel();

	//void setModel(const char *sm); // see later


	void setLikelihoodWindow(int w);

	void filter(float *observation, int observationSize);

	const char *getLikeliest();

	float *getLikelihoods(int nLabels);

}