extern "C" {

	void addPhrase(const char *label, const char **colNames, int nCols, float *phrase, int phraseSize);

	const char *getLastPhrase();

	int getSetSize();

	//const char **getSetLabels();

	void train(int nbOfGaussians);

	int getNbOfModels();

	const char *getModels();

	void setModels(const char *sm);

	void clearModels();

	const char *getTrainingSet();

	void setTrainingSet(const char *sts);

	void clearTrainingSet();

	void clearLabel(const char *label);



	void setLikelihoodWindow(int w);

	void filter(float *observation, int observationSize);

	const char *getLikeliest();

	float *getLikelihoods(int nLabels);

}