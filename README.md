[![Build status](https://ci.appveyor.com/api/projects/status/465nxgtsf4mtuay3?svg=true)](https://ci.appveyor.com/project/Still/psvdecryptcore)

# THE PROGRAM IS STILL UNDER DEVELOPMENT.

There are still several issues that need to be ironed out first. I'd recommend sticking to the original for now.

# PsvDecryptCore
Video file decryptor for a famous online developer training site. Originally developed by [KevinWang15](https://github.com/KevinWang15/).

# Windows / Ubuntu Binaries
Download the latest release [here](https://ci.appveyor.com/project/Still/psvdecryptcore/build/artifacts).

# What's different?
The original application was poorly designed with little to no multi-thread operation in mind. 
This version is re-written from scratch with following improvements,

* Better multi-threaded performance (tested up to 200% improvement)
* No more messing around with `DataTable`, instead uses Entity Framework context for easier database access.
* Full & detailed logging support

# Speed difference

| Courses | Total Size | Speed (Core) | Speed (Original) |
|---------|------------|--------------|------------------|
| 5 courses | 3.83 GB | 00:00:26.0487531 | 00:01:18.2927028|

# Usage

0. Make sure you have downloaded the desired courses using their Offline Viewer.

*(Linux user must also specify where the database is located by creating an environment variable `psv`)*
1. Download and unzip version corresponding to your OS.
2. Browse to the unzipped folder.
3. Execute by double clicking (or via command-line for Ubuntu users).
4. Follow the console instruction.

# Disclaimer
Please only use it for your convenience so that you can watch the courses on your devices offline or for educational purposes.

Piracy is strictly prohibited. Decrypted videos should not be uploaded to open servers, torrents, or other methods of mass distribution. 
Any consequences resulting from misuse of this tool are to be taken by the user.
