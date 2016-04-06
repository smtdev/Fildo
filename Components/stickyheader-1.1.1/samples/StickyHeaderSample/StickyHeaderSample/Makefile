top_srcdir=../../../..
TOP=$(top_srcdir)

include $(top_srcdir)/commands.make

PROJECTNAME=StickyHeaderSample
SLNFILE=../StickyHeaderSample.sln

BUILDCONFIGURATION=Release
BUILDOUTPUTDIR = bin/$(BUILDCONFIGURATION)

ASSEMBLYFILE=StickyHeaderSample.dll
BUILDOUTPUT = $(BUILDOUTPUTDIR)/$(ASSEMBLYFILE)

all: $(BUILDOUTPUT)

$(BUILDOUTPUT) : $(shell find . -type f -name '*.cs' -not -path "./obj/*" -not -path "") \
	$(shell find . -type f -name '*.xml' -not -path "./obj/*" -not -path "./bin/*") \
        $(PROJECTNAME).csproj

	$(NUGET) restore $(SLNFILE)

	$(MDTOOL) -v build "--configuration:$(BUILDCONFIGURATION)" $(SLNFILE) -p:$(PROJECTNAME)

clean:
	rm -rf bin/ obj/ 


.PHONY : all clean
