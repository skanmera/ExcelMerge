# NetDiff

This is the C # implementation of the Diff algorithm.

<br/>

## Usage

```cs
using System;
using System.Collections.Generic;
using System.Linq;
using NetDiff;

namespace NetDiffSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var str1 = "string";
            var str2 = "strength";

            IEnumerable<DiffResult<char>> results = DiffUtil.Diff(str1, str2);

            results.ToList().ForEach(r => Console.WriteLine(r.ToFormatString()));
        }
    }
}
```
```cs
= s
= t
= r
- i
+ e
= n
= g
+ t
+ h
```

<br/>

## Option

### EqualityComparer
  
Specify IEqualityComparer to be used for comparing equality.

```cs
class Program
{
    static void Main(string[] args)
    {
        var str1 = "string";
        var str2 = "stRength";

        var option = new DiffOption<char>();
        option.EqualityComparer = new CaseInsensitiveComparer();

        IEnumerable<DiffResult<char>> results = DiffUtil.Diff(str1, str2, option);

        results.ToList().ForEach(r => Console.WriteLine(r.ToFormatString()));
        Console.Read();
    }

}

class CaseInsensitiveComparer : IEqualityComparer<char>
{
    public bool Equals(char x, char y)
    {
        return x.ToString().ToLower().Equals(y.ToString().ToLower());
    }

    public int GetHashCode(char obj)
    {
        return obj.ToString().ToLower().GetHashCode();
    }
}
```
```cs
= s
= t
= R
+ e
- i
= n
= g
+ t
+ h
```

### Order

Specify order of  Insert and Delete from the shortest path in the edit graph.

#### LazyInsertFirst
```cs
var str1 = "aaa";
var str2 = "bbb";

var option = new DiffOption<char>();
option.Order = DiffOrder.LazyInsertFirst;

IEnumerable<DiffResult<char>> results = DiffUtil.Diff(str1, str2, option);

```
```
+ b
- a
+ b
- a
+ b
- a
```


#### LazyDeleteFirst
```cs
var str1 = "aaa";
var str2 = "bbb";

var option = new DiffOption<char>();
option.Order = DiffOrder.LazyDeleteFirst;

IEnumerable<DiffResult<char>> results = DiffUtil.Diff(str1, str2, option);
```
```
- a
+ b
- a
+ b
- a
+ b
```

#### GreedyInsertFirst
```cs
var str1 = "aaa";
var str2 = "bbb";

var option = new DiffOption<char>();
option.Order = DiffOrder.GreedyInsertFirst;

IEnumerable<DiffResult<char>> results = DiffUtil.Diff(str1, str2, option);
```
```
+ b
+ b
+ b
- a
- a
- a
```

#### GreedyDeleteFirst
```cs
var str1 = "aaa";
var str2 = "bbb";

var option = new DiffOption<char>();
option.Order = DiffOrder.GreedyDeleteFirst;

IEnumerable<DiffResult<char>> results = DiffUtil.Diff(str1, str2, option);
```
```
- a
- a
- a
+ b
+ b
+ b
```

### Performance

Specify the maximum number of nodes that can exist at once at the edit graph.
The lower the number, the better the performance, but the redundant differences increase.
The default is 1000.

```cs
var txt1 = Enumerable.Repeat("aaa", 10000);
var txt2 = Enumerable.Repeat("bbb", 10000);

var option = new DiffOption<string>();

var stopwatch = new System.Diagnostics.Stopwatch();

option.Limit = 1000;
stopwatch.Start();
DiffUtil.Diff(txt1, txt2, option);
stopwatch.Stop();

Console.WriteLine(stopwatch.Elapsed);

option.Limit = 100;
stopwatch.Restart();
DiffUtil.Diff(txt1, txt2, option);
stopwatch.Stop();

Console.WriteLine(stopwatch.Elapsed);
```
```cs
00:00:08.3869959
00:00:00.6112575
```

<br/>

## Optimize

Convert deleted/inserted to modified.

```cs
/*
    src a  a  a        a a a 
    dst   b  b  b  ->  b b b
         - + - + -     M M M
*/
var results = DiffUtil.Diff("aaa", "bbb", option);
var optimized = DiffUtil.OptimizeCaseDeletedFirst(results);
```

## License

[MIT License](http://opensource.org/licenses/MIT)
