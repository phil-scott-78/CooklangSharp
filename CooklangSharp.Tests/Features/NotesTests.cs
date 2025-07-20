using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests.Features;

public class NotesTests
{
    [Fact]
    public void ParsesNotesCorrectly()
    {
        var source = """
            > Don't burn the roux!
            
            Mash @potato{2%kg} until smooth.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Sections.Count.ShouldBe(1);
        
        var section = result.Recipe.Sections[0];
        section.Name.ShouldBeNull(); // Default section
        section.Content.Count.ShouldBe(2);
        
        // First item should be a note
        var noteContent = section.Content[0] as NoteContent;
        noteContent.ShouldNotBeNull();
        noteContent.Type.ShouldBe("text");
        noteContent.Value.ShouldBe("Don't burn the roux!");
        
        // Second item should be a step
        var stepContent = section.Content[1] as StepContent;
        stepContent.ShouldNotBeNull();
        stepContent.Type.ShouldBe("step");
        stepContent.Step.Number.ShouldBe(1);
    }

    [Fact]
    public void ParsesMultipleNotes()
    {
        var source = """
            > First note about preparation.
            > Second note about safety.
            
            Start cooking.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var section = result.Recipe.Sections[0];
        section.Content.Count.ShouldBe(3);
        
        var note1 = section.Content[0] as NoteContent;
        note1.ShouldNotBeNull();
        note1.Value.ShouldBe("First note about preparation.");
        
        var note2 = section.Content[1] as NoteContent;
        note2.ShouldNotBeNull();
        note2.Value.ShouldBe("Second note about safety.");
        
        var step = section.Content[2] as StepContent;
        step.ShouldNotBeNull();
    }

    [Fact]
    public void ParsesNoteWithWhitespace()
    {
        var source = """
            >   Note with extra spaces   
            
            Mix ingredients.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var note = result.Recipe.Sections[0].Content[0] as NoteContent;
        note.ShouldNotBeNull();
        note.Value.ShouldBe("Note with extra spaces");
    }

    [Fact]
    public void ParsesEmptyNote()
    {
        var source = """
            >
            
            Mix ingredients.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var note = result.Recipe.Sections[0].Content[0] as NoteContent;
        note.ShouldNotBeNull();
        note.Value.ShouldBe("");
    }

    [Fact]
    public void ParsesNoteWithSpecialCharacters()
    {
        var source = """
            > Temperature should be 350°F (175°C) - be careful!
            
            Bake for ~{25%minutes}.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var note = result.Recipe.Sections[0].Content[0] as NoteContent;
        note.ShouldNotBeNull();
        note.Value.ShouldBe("Temperature should be 350°F (175°C) - be careful!");
    }

    [Fact]
    public void ParsesNoteInSection()
    {
        var source = """
            = Preparation
            
            > Ensure all ingredients are at room temperature.
            
            Mix @flour{200%g} and @water{100%ml}.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var section = result.Recipe.Sections[0];
        section.Name.ShouldBe("Preparation");
        section.Content.Count.ShouldBe(2);
        
        var note = section.Content[0] as NoteContent;
        note.ShouldNotBeNull();
        note.Value.ShouldBe("Ensure all ingredients are at room temperature.");
    }

    [Fact]
    public void ParsesNoteBetweenSteps()
    {
        var source = """
            Mix @flour{200%g} and @water{100%ml}.
            
            > Let the dough rest for 30 minutes.
            
            Knead the dough for ~{5%minutes}.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var section = result.Recipe.Sections[0];
        section.Content.Count.ShouldBe(3);
        
        var step1 = section.Content[0] as StepContent;
        step1.ShouldNotBeNull();
        step1.Step.Number.ShouldBe(1);
        
        var note = section.Content[1] as NoteContent;
        note.ShouldNotBeNull();
        note.Value.ShouldBe("Let the dough rest for 30 minutes.");
        
        var step2 = section.Content[2] as StepContent;
        step2.ShouldNotBeNull();
        step2.Step.Number.ShouldBe(2);
    }

    [Fact]
    public void ParsesNoteWithPunctuation()
    {
        var source = """
            > Important: Don't overcook! Watch carefully...
            
            Cook for ~{5%minutes}.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var note = result.Recipe.Sections[0].Content[0] as NoteContent;
        note.ShouldNotBeNull();
        note.Value.ShouldBe("Important: Don't overcook! Watch carefully...");
    }
}