# FtlDB
## Please dont actually use this for anything. The whole idea is basically a security nightmare

This project is just for fun. The entire idea is tables are represented by CLR types. To do this when a table is created we generate some C# in the shape of the table and compile it. This class is what will be used for serializing and deserializing rows (and eventually for queries potentially?). Data passed in through an API call is directly used to generate C# classes that get compiled and used all at runtime.
