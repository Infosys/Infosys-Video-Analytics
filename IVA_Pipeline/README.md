
# Infosys Video Analytics

## Table of Contents
- [Installation](#-Installation)
- [Prerequisites](#-Prerequisites)
- [Build and Test](#-build-and-test)
- [Tests](#tests)
- [Additional Features](#additional-features)

### Installation

How to build the project:
```shell
#Open a terminal (Command Prompt or PowerShell for Windows, Terminal for macOS or Linux)

#Ensure Git is installed
Visit https://git-scm.com to download and install console Git if not already installed

# Clone the Repository
  git clone link copied from the repo

# Check if .Net is installed
  dotnet --version  # Check the installed version of .NET SDK (Ensure .NET 6.0 is installed)
# Visit the official Microsoft website to install or update it if necessary

# Restore dependencies
  dotnet restore

# Compile the project
  dotnet build
```

This section guides users through setting up and running the IVA on their system.

### Prerequisites (Install Dependencies)

- **7-Zip or WinZip**: Required to unzip the binaries folder.
- **.NET 8.0**: Ensure .NET 8.0 is installed on the computer where the pipeline will be run.
- **Windows OS**: Windows 10 or greater.
- **Windows Server**: If running on a server, Windows Server 2012 R2 or greater.
- **VLC Player**: Required to view the video output. Ensure the latest version is installed.
Few installations inside process loader project to run the solution:
- **Npgsql 7.0.0**: Install from [Npgsql](http://www.npgsql.org/)
- **Npgsql.EntityFrameworkCore.PostgreSQL 7.0.0**: Install from [NuGet](http://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL)
- **EMGU CV**: Install from [EMGU CV](http://www.emgu.com/wiki/index.php)
- **FFmpeg**: Install the `ffmpeg.exe` application from [FFmpeg](https://github.com/FFmpeg/FFmpeg)

🚀 Build and Test (Usage) (IVA SetUp)

1. **Extract the IVA-open source archive**: Use 7-Zip or WinZip to unzip the binaries folder to a directory of your choice.
2. **Build the Solution**:
   - Navigate to the IVA directory.
   - Clean and build the solution.
   - Set the startup project to the FrameGrabber build the solution and again make process loader project as startup project and rebuild the entire solution.
3. **Configuration Files**:
   - Open Process loader folder.
   - Modify the following configuration files as needed:
     - `LiSettings.json`: Specify the complete path of the LIF adapter DLL from the references folder.(All 4 DLL's)
     - `Process.Config`, `Device.json`, `config.ini`: Used for Python network execution, located in the Configuration folder.
     - `ModelType.xml`: Located in the XML folder from AIModels inside Prediction folder, In xml we can integrate the API and test the usecase.
     - `PythonModelExecutor.py`: The entry point for all Python network executions. Modifications to this file do not require a solution rebuild.
     - `Appsetting.json`: Specify the path to `device.json` from process loader inside Configurations folder and SQL connection strings if needed.
4. **Testing**:
   - Update `device.json`, `appsettings.json`, `lisettings.json`, and the XML file to specify the model API to be tested.
   - Place the input video file or image in the designated input directory.
   - Execute `processloader.exe` to initiate the command prompt. FFmpeg will start, and the output will be generated as `pd.flv`.

### Tests:

#### Steps for Local Python Inference with IVA Setup
Download the InfosysModelInferenceLibrary folder from the https://github.com/Infosys/Infosys-Model-Inference-Library
1. Required Files: (Files inside Process loader bin folder which is infosysmodelinferencelibrary(imil))
   - custom_model_loader.py
   - mil_config.json
   - PythonModelExecutor.py
   - Python >=3.9 (Ensure it is installed on the machine to run Python inference)
2. Installation Steps:
   - Install Necessary Packages,
   - Verify installed packages using,
   - Check the Requirements.txt inside the infosysmodelinferencelibrary folder and
   - Install any missing packages.
   - Check the References folder inside imil folder for deatiled explanation on the infosysmodelinferencelibrary.

Navigate to the model inference directory and locate the wheel file.

- In the command prompt, run:
- Replace <path_to_wheel_file> with the actual path of the wheel file.
- You can perform similar tests with your model using the necessary Python environment. Ensure the following given steps are completed:
- Set Up the Python Environment

4. Run the Model:
- Execute the necessary scripts to perform inference with your model.

For detailed documentation and instructions on the local execution of IMiL, please refer to
 https://github.com/Infosys/Infosys-Model-Inference-Library

### Steps for API Testing with IVA Setup

1. Deploy the API:

   - Based on the provided IVA request and response structure (refer to [References](#-References) section), deploy the API for testing.
2. Configure API Endpoint:

   - Specify the API endpoint in the XML file.
   - Set the keyword for the model you are testing in the predictionModel field in device.json.
3. Modify Configuration Files:

   - Update the following configuration files in Processloader project as needed:
   - LiSettings.json: Specify the path of the LIF adapter DLL from the references folder.
   - Process.Config
   - Device.json
   - config.ini (used for Python net execution)
   - These files are located in the Configuration folder.
4. Update XML File:

   - ModelType.xml file is located in the XML folder from AIModels inside Prediction folder.
5. Python Net Execution:

   - PythonModelExecutor.py is the entry file for all Python net executions. Modifying this file does not require a solution build.
6. App Settings:

   - In Appsetting.json, specify the path of device.json from process loader project inside Configuration folder.

Execute `processloader.exe` to initiate the command prompt. FFmpeg will start, and the output will be generated as `pd.flv`.

