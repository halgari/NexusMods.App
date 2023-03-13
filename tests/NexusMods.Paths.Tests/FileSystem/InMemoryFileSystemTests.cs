using System.Text;
using FluentAssertions;
using NexusMods.Paths.Tests.AutoData;

namespace NexusMods.Paths.Tests.FileSystem;

public class InMemoryFileSystemTests
{
    [SkippableTheory, AutoFileSystem]
    public void Test_GetFileEntry(InMemoryFileSystem fs, AbsolutePath path)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.AddEmptyFile(path);
        var entry = fs.GetFileEntry(path);
        entry.Path.Should().Be(path);
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_GetFileEntry_MissingFile(InMemoryFileSystem fs,
        AbsolutePath path)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        var entry = fs.GetFileEntry(path);
        entry.Path.Should().Be(path);
    }

    [SkippableTheory, AutoFileSystem]
    public async Task Test_OpenFile(InMemoryFileSystem fs, AbsolutePath path, string contents)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        var bytes = Encoding.UTF8.GetBytes(contents);

        fs.AddFile(path, bytes);

        await using var stream = fs.ReadFile(path);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        var actualBytes = new byte[ms.Length];
        ms.Position = 0;

        var count = await ms.ReadAsync(actualBytes);
        count.Should().Be(bytes.Length);

        actualBytes.Should().BeEquivalentTo(bytes);
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_OpenFile_FileNotFound(InMemoryFileSystem fs, AbsolutePath path)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.Invoking(x => x.ReadFile(path))
            .Should()
            .Throw<FileNotFoundException>();
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_CreateDirectory(InMemoryFileSystem fs, AbsolutePath path)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.CreateDirectory(path);
        fs.DirectoryExists(path).Should().BeTrue();
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_CreateDirectory_SubDirectory(InMemoryFileSystem fs,
        AbsolutePath parentDirectory,
        string subDirectoryName)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        var subDirectory = parentDirectory.CombineUnchecked(subDirectoryName);

        fs.CreateDirectory(subDirectory);

        fs.DirectoryExists(subDirectory).Should().BeTrue();
        fs.DirectoryExists(parentDirectory).Should().BeTrue();
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_DirectoryExists(InMemoryFileSystem fs, AbsolutePath path)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.DirectoryExists(path).Should().BeFalse();
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_FileExists(InMemoryFileSystem fs, AbsolutePath path)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.FileExists(path).Should().BeFalse();
        fs.AddEmptyFile(path);
        fs.FileExists(path).Should().BeTrue();
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_DeleteFile(InMemoryFileSystem fs, AbsolutePath path)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.FileExists(path).Should().BeFalse();
        fs.AddEmptyFile(path);
        fs.FileExists(path).Should().BeTrue();
        fs.DeleteFile(path);
        fs.FileExists(path).Should().BeFalse();
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_DeleteFile_NotExists(InMemoryFileSystem fs, AbsolutePath path)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.FileExists(path).Should().BeFalse();
        fs.Invoking(x => x.DeleteFile(path))
            .Should()
            .Throw<FileNotFoundException>();
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_DeleteDirectory(InMemoryFileSystem fs, AbsolutePath path)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.DirectoryExists(path).Should().BeFalse();
        fs.CreateDirectory(path);
        fs.DirectoryExists(path).Should().BeTrue();
        fs.DeleteDirectory(path, false);
        fs.DirectoryExists(path).Should().BeFalse();
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_DeleteDirectory_NotExist(InMemoryFileSystem fs, AbsolutePath path)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.Invoking(x => x.DeleteDirectory(path, false))
            .Should()
            .Throw<DirectoryNotFoundException>();
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_DeleteDirectory_Recursive(
        InMemoryFileSystem fs,
        AbsolutePath directory,
        string subDirectoryName,
        string fileName1,
        string fileName2)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.CreateDirectory(directory);

        var subDirectory = directory.CombineUnchecked(subDirectoryName);
        fs.CreateDirectory(subDirectory);

        var file1 = directory.CombineUnchecked(fileName1);
        var file2 = subDirectory.CombineUnchecked(fileName2);

        fs.AddEmptyFile(file1);
        fs.AddEmptyFile(file2);

        fs.DirectoryExists(directory).Should().BeTrue();
        fs.DirectoryExists(subDirectory).Should().BeTrue();
        fs.FileExists(file1).Should().BeTrue();
        fs.FileExists(file2).Should().BeTrue();

        fs.DeleteDirectory(directory, true);

        fs.DirectoryExists(directory).Should().BeFalse();
        fs.DirectoryExists(subDirectory).Should().BeFalse();
        fs.FileExists(file1).Should().BeFalse();
        fs.FileExists(file2).Should().BeFalse();
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_DeleteDirectory_NotEmpty(
        InMemoryFileSystem fs,
        AbsolutePath directory,
        string fileName)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.CreateDirectory(directory);

        var file = directory.CombineUnchecked(fileName);
        fs.AddEmptyFile(file);

        fs.DirectoryExists(directory).Should().BeTrue();
        fs.FileExists(file).Should().BeTrue();

        fs.Invoking(x => x.DeleteDirectory(directory, false))
            .Should()
            .Throw<IOException>();
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_MoveFile_NoOverwrite(InMemoryFileSystem fs, AbsolutePath source, AbsolutePath dest, byte[] contents)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.FileExists(source).Should().BeFalse();
        fs.FileExists(dest).Should().BeFalse();

        fs.AddFile(source, contents);
        fs.FileExists(source).Should().BeTrue();

        fs.MoveFile(source, dest, false);
        fs.FileExists(source).Should().BeFalse();
        fs.FileExists(dest).Should().BeTrue();

        fs.GetFileEntry(dest).Size.Should().Be(Size.From(contents.Length));
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_MoveFile_NoOverwriteExists(InMemoryFileSystem fs,
        AbsolutePath source, AbsolutePath dest, byte[] contents)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.FileExists(source).Should().BeFalse();
        fs.FileExists(dest).Should().BeFalse();

        fs.AddFile(source, contents);
        fs.AddEmptyFile(dest);

        fs.FileExists(source).Should().BeTrue();
        fs.FileExists(dest).Should().BeTrue();

        fs.Invoking(x => x.MoveFile(source, dest, false))
            .Should()
            .Throw<IOException>();
    }

    [SkippableTheory, AutoFileSystem]
    public void Test_MoveFile_Overwrite(InMemoryFileSystem fs,
        AbsolutePath source, AbsolutePath dest, byte[] contents)
    {
        Skip.IfNot(OperatingSystem.IsLinux());
        fs.FileExists(source).Should().BeFalse();
        fs.FileExists(dest).Should().BeFalse();

        fs.AddFile(source, contents);
        fs.AddEmptyFile(dest);

        fs.FileExists(source).Should().BeTrue();
        fs.FileExists(dest).Should().BeTrue();

        fs.MoveFile(source, dest, true);
        fs.FileExists(source).Should().BeFalse();
        fs.FileExists(dest).Should().BeTrue();

        fs.GetFileEntry(dest).Size.Should().Be(Size.From(contents.Length));
    }
}
