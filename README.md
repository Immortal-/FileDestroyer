# FileDestroyer

I wanted to write a tool to help me remove files from my system by digitally shredding themSo I wrote this application. It's multithreaded, I have optimized the code as much as I can and still be 100% positive the file does not exist any longer. This application is using .net framework 2.0
-
**Video:**
https://vimeo.com/134587060


**Q & A**

**Q:**
Nice source, but I'm a little confused why you are encrypting the temporary file in memory, and only writing zeros to disk...  Kind of defeats the purpose of the encryption, right? :P 

**A:**
wanted to make sure that no trails of data were left behind for lets say a forensic data analysis team. That way if there were tiny bits in the ram that didnt get changed to zeros do to some kind of anomaly the data is encrypted.  So I take the size of the file to a byte[] and then encrypt the one in the memory and overwrite with 0's to make sure it's gone then delete the file. 

**Q:**
What size file will this support? 

**A:**
Not sure the most I tested with was 15 files that were 25.6MB and a single file that was 996MB. The program did not crash it removed all the smaller files first and idled as it deleted the big one, Took about 3mins.
