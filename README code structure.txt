Code Structure of WebTableExtraction Project.


Utils Folder: the basic classes used by all algorithms. 
	— Cell.cs represent a cell in a record
	— Record.cs represents a record
	— Line.cs represents a line in the input list
	— Parameter.cs records all the parameters for the algorithms
	— Seperators records all the delimiters to separate a line into a list of tokens
	— Table.cs is a table
	— GlobalScoringInfo.cs loads the statistics files into memory for a set of experiment
	— LocalScoringInfo.cs caches and calculates the distances for a specific table
	— Table_Line_Conversion.cs is for running on Cosmos, convert a table into one line


Baseline Folder: the implementation of Google’s Algorithm
	— Baseline.cs is the entry point
	— Splitting.cs is the first phase
	— Alignment.cs is the second phase
	— Refinment.cs is the third phase
	— FiledSummary.cs is the data structure they use

Judie Folder: the implementation of Judie (Eli’s)
	

Extraction Folder: our main algorithm
	—MSAAppro.cs: is our main algorithm (2-approximation)
	—SingleAlignment.cs: is to align a line to a record
	—InterestingSegmentation.cs: is to calculate all the necessary information, including free distance, h function, for an anchor line


	— DoOneTable.cs is just a wrapper that needed to do multithreading, where each thread will call this class
	— MSASplitting.cs is one method to determine the number of columns in unsupervised setting. this is not used anymore


ExtractionAStar: The A star search procedure that MSAAppro calls
	—MSAAStar.cs: the A star procedure
	—AStarNode.cs: each node in the A star search
