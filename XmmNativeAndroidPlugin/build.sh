cd ./jni

ndk-build clean && ndk-build

cd ../libs

for i in $(ls -d */); do
	ff=$(echo ${i%%/});
	mkdir -p ../../Assets/Plugins/Android-"$ff"
	cp -r "$ff"/* ../../Assets/Plugins/Android-"$ff"
done