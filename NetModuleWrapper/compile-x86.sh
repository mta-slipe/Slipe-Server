sudo apt-get install g++-multilib gcc-multilib -y
sudo apt-get install linux-libc-dev linux-libc-dev:i386 -y

mkdir -p x64
g++ -c -o x64/wrapperFunctions.o wrapperFunctions.cpp -fPIC
g++ -c -o x64/NetWrapper.o NetWrapper.cpp -fPIC
g++ -c -o x64/CDynamicLibrary.o mta/core/CDynamicLibrary.cpp -fPIC
gcc x64/wrapperFunctions.o x64/NetWrapper.o x64/CDynamicLibrary.o -shared -o x64/NetModuleWrapper.so

cp x64/NetModuleWrapper.so ../SlipeServer.Net/Binaries/x64/

mkdir -p x86
g++ -c -m32 -o x86/wrapperFunctions.o wrapperFunctions.cpp -fPIC
g++ -c -m32 -o x86/NetWrapper.o NetWrapper.cpp -fPIC
g++ -c -m32 -o x86/CDynamicLibrary.o mta/core/CDynamicLibrary.cpp -fPIC
gcc -m32 x86/wrapperFunctions.o x86/NetWrapper.o x86/CDynamicLibrary.o -shared -o x86/NetModuleWrapper.so

cp x86/NetModuleWrapper.so ../SlipeServer.Net/Binaries/x86/
