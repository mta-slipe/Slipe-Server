sudo apt-get install gcc-arm-linux-gnueabihf g++-arm-linux-gnueabihf -y
sudo apt-get install gcc-aarch64-linux-gnu g++-aarch64-linux-gnu -y
sudo apt-get install libncurses5-dev libncursesw5-dev -y
          
mkdir -p arm64
aarch64-linux-gnu-g++ -fPIC -shared wrapperFunctions.cpp NetWrapper.cpp mta/core/CDynamicLibrary.cpp -o arm64/NetModuleWrapper.so -shared
cp arm64/NetModuleWrapper.so ../SlipeServer.Net/Binaries/arm64/

mkdir -p arm32
arm-linux-gnueabihf-g++ -fPIC -shared wrapperFunctions.cpp NetWrapper.cpp mta/core/CDynamicLibrary.cpp -o arm32/NetModuleWrapper.so -shared
cp arm32/NetModuleWrapper.so ../SlipeServer.Net/Binaries/arm32/
