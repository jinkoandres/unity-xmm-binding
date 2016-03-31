LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_CPPFLAGS := -DANDROID_VERSION

LOCAL_MODULE := XMMEngine

CODE_PATH := ../engine

LOCAL_XMM_SRC := $(LOCAL_PATH)/$(CODE_PATH)/xmm/src

LOCAL_JSONCPP_SRC := $(LOCAL_PATH)/$(CODE_PATH)/xmm/dependencies/jsoncpp

LOCAL_C_INCLUDES := $(LOCAL_PATH)/$(CODE_PATH)/include \
		$(LOCAL_JSONCPP_SRC)/include $(LOCAL_XMM_SRC)

LOCAL_SRC_FILES := $(CODE_PATH)/src/XMMEngine.cpp \
		$(CODE_PATH)/src/XMMEngineCore.cpp \
		$(LOCAL_XMM_SRC)/core/common/xmmAttribute.cpp \
		$(LOCAL_XMM_SRC)/core/common/xmmJson.cpp \
		$(LOCAL_XMM_SRC)/core/distributions/xmmGaussianDistribution.cpp \
		$(LOCAL_XMM_SRC)/core/model/xmmModelSharedParameters.cpp \
		$(LOCAL_XMM_SRC)/core/model/xmmModelSingleClass.cpp \
		$(LOCAL_XMM_SRC)/core/trainingset/xmmPhrase.cpp \
		$(LOCAL_XMM_SRC)/core/trainingset/xmmTrainingSet.cpp \
		$(LOCAL_XMM_SRC)/models/gmm/xmmGmm.cpp \
		$(LOCAL_XMM_SRC)/models/gmm/xmmGmmParameters.cpp \
		$(LOCAL_XMM_SRC)/models/gmm/xmmGmmSingleClass.cpp \
		$(LOCAL_XMM_SRC)/models/hmm/xmmHierarchicalHmm.cpp \
		$(LOCAL_XMM_SRC)/models/hmm/xmmHmmParameters.cpp \
		$(LOCAL_XMM_SRC)/models/hmm/xmmHmmSingleClass.cpp \
		$(LOCAL_XMM_SRC)/models/kmeans/xmmKMeans.cpp \
		$(LOCAL_XMM_SRC)/models/kmeans/xmmKMeansParameters.cpp \
		$(LOCAL_JSONCPP_SRC)/src/json_reader.cpp \
		$(LOCAL_JSONCPP_SRC)/src/json_value.cpp \
		$(LOCAL_JSONCPP_SRC)/src/json_writer.cpp

LOCAL_LDLIBS += -latomic

include $(BUILD_SHARED_LIBRARY)
