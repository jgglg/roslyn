' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces
Imports Microsoft.CodeAnalysis.Options
Imports Microsoft.CodeAnalysis.Simplification
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.Editor.UnitTests.Simplification
    Public Class TypeNameSimplifierTest
        Inherits AbstractSimplificationTests

#Region "Normal CSharp Tests"

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyAllNodes_SimplifyTypeName()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    using System;
                    namespace Root 
                    {
                        class A 
                        {
                            {|SimplifyParent:System.Exception|} c;
                        }
                    }
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                    using System;
                    namespace Root 
                    {
                        class A 
                        {
                            Exception c;
                        }
                    }
                </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyAllNodes_SimplifyReceiver1()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                class C
                {
                    void M(C other)
                    {
                        {|SimplifyParent:other.M|}(null);
                    }
                }
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                class C
                {
                    void M(C other)
                    {
                        other.M(null);
                    }
                }
                </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyAllNodes_SimplifyReceiver2()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                class C
                {
                    void M(C other)
                    {
                        {|SimplifyParent:this.M|}(null);
                    }
                }
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                class C
                {
                    void M(C other)
                    {
                        M(null);
                    }
                }
                </text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyAllNodes_SimplifyNestedType()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
using System;

class Preserve
{
	public class X
	{
		public static int Y;
	}
}

class Z<T> : Preserve
{
}

static class M
{
	public static void Main()
	{
		int k = {|SimplifyParent:Z<float>.X.Y|};
	}
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
using System;

class Preserve
{
	public class X
	{
		public static int Y;
	}
}

class Z<T> : Preserve
{
}

static class M
{
	public static void Main()
	{
		int k = Preserve.X.Y;
	}
}]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyAllNodes_SimplifyNestedType2()
            ' Simplified type is in a different namespace.

            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
using System;

namespace N1
{
    class Preserve
    {
        public class X
        {
            public static int Y;
        }
    }
}

namespace P
{
    class NonGeneric : N1.Preserve
    {
    }
}

static class M
{
    public static void Main()
    {
        int k = P.NonGeneric.{|SimplifyParent:X|}.Y;
    }
}               </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
using System;

namespace N1
{
    class Preserve
    {
        public class X
        {
            public static int Y;
        }
    }
}

namespace P
{
    class NonGeneric : N1.Preserve
    {
    }
}

static class M
{
    public static void Main()
    {
        int k = N1.Preserve.X.Y;
    }
}</text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyAllNodes_SimplifyNestedType3()
            ' Simplified type is in a different namespace, whose names have been imported with a usings statement.

            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
using System;

namespace N1
{
    class Preserve
    {
        public class X
        {
            public static int Y;
        }
    }
}

namespace P
{
    class NonGeneric : N1.Preserve
    {
    }
}

namespace R
{
    using N1;

    static class M
    {
        public static void Main()
        {
            int k = P.NonGeneric.{|SimplifyParent:X|}.Y;
        }
    }
}               </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
using System;

namespace N1
{
    class Preserve
    {
        public class X
        {
            public static int Y;
        }
    }
}

namespace P
{
    class NonGeneric : N1.Preserve
    {
    }
}

namespace R
{
    using N1;

    static class M
    {
        public static void Main()
        {
            int k = Preserve.X.Y;
        }
    }
}</text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyAllNodes_SimplifyNestedType4()
            ' Highly nested type simplified to another highly nested type.

            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
using System;

namespace N1
{
    namespace N2
    {
        public class Outer
        {
            public class Preserve
            {
                public class X
                {
                    public static int Y;
                }
            }
        }
    }

}

namespace P1
{
    namespace P2
    {
        public class NonGeneric : N1.N2.Outer.Preserve
        {
        }
    }
}

namespace Q1
{
    using P1.P2;

    namespace Q2
    {
        class Generic<T> : NonGeneric
        {
        }
    }
}

namespace R
{
    using Q1.Q2;

    static class M
    {
        public static void Main()
        {
            int k = Generic<int>.{|SimplifyParent:X|}.Y;
        }
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
using System;

namespace N1
{
    namespace N2
    {
        public class Outer
        {
            public class Preserve
            {
                public class X
                {
                    public static int Y;
                }
            }
        }
    }

}

namespace P1
{
    namespace P2
    {
        public class NonGeneric : N1.N2.Outer.Preserve
        {
        }
    }
}

namespace Q1
{
    using P1.P2;

    namespace Q2
    {
        class Generic<T> : NonGeneric
        {
        }
    }
}

namespace R
{
    using Q1.Q2;

    static class M
    {
        public static void Main()
        {
            int k = N1.N2.Outer.Preserve.X.Y;
        }
    }
}]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyAllNodes_SimplifyNestedType5()
            ' Name requiring multiple iterations of nested type simplification.

            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
using System;

namespace N1
{
    namespace N2
    {
        public class Outer
        {
            public class Preserve
            {
                public class X
                {
                    public static int Y;
                }
            }
        }
    }
    
}

namespace P1
{
    using N1.N2;
    namespace P2
    {
        public class NonGeneric : Outer
        {
            public class NonGenericInner : Outer.Preserve
            {
            }
        }
    }
}

namespace Q1
{
    using P1.P2;

    namespace Q2
    {
        class Generic<T> : NonGeneric
        {
        }
    }
}

namespace R
{
    using N1.N2;
    using Q1.Q2;

    static class M
    {
        public static void Main()
        {
            int k = Generic<int>.NonGenericInner.{|SimplifyParent:X|}.Y;
        }
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
using System;

namespace N1
{
    namespace N2
    {
        public class Outer
        {
            public class Preserve
            {
                public class X
                {
                    public static int Y;
                }
            }
        }
    }
    
}

namespace P1
{
    using N1.N2;
    namespace P2
    {
        public class NonGeneric : Outer
        {
            public class NonGenericInner : Outer.Preserve
            {
            }
        }
    }
}

namespace Q1
{
    using P1.P2;

    namespace Q2
    {
        class Generic<T> : NonGeneric
        {
        }
    }
}

namespace R
{
    using N1.N2;
    using Q1.Q2;

    static class M
    {
        public static void Main()
        {
            int k = Outer.Preserve.X.Y;
        }
    }
}]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyAllNodes_SimplifyStaticMemberAccess()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
using System;

class Preserve
{
	public static int Y;
}

class Z<T> : Preserve
{
}

static class M
{
	public static void Main()
	{
		int k = {|SimplifyParent:Z<float>.Y|};
	}
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
using System;

class Preserve
{
	public static int Y;
}

class Z<T> : Preserve
{
}

static class M
{
	public static void Main()
	{
		int k = Preserve.Y;
	}
}]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyAllNodes_SimplifyQualifiedName()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
using System;

class A
{
    public static class B { }
}

class C : A
{
}

namespace N1
{
    static class M
    {
        public static {|SimplifyParent:C.B|} F()
        {
            return null;
        }
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
using System;

class A
{
    public static class B { }
}

class C : A
{
}

namespace N1
{
    static class M
    {
        public static A.B F()
        {
            return null;
        }
    }
}]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyAllNodes_SimplifyAliasStaticMemberAccess()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
using System;

class Preserve
{
    public static int Y;
}

class NonGeneric : Preserve
{
}

namespace N1
{
    using X = NonGeneric;

    static class M
    {
        public static void Main()
        {
            int k = {|SimplifyParent:X|}.Y;
        }
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
using System;

class Preserve
{
    public static int Y;
}

class NonGeneric : Preserve
{
}

namespace N1
{
    using X = NonGeneric;

    static class M
    {
        public static void Main()
        {
            int k = Preserve.Y;
        }
    }
}]]></text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyNot_Delegate1()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                using System;
                class A
                {
                    static void Del() { }
                    class B
                    {
                        delegate void Del();
                        void Boo()
                        {
                            Del d = new Del(A.{|SimplifyParent:Del|}); 
                            A.{|SimplifyParent:Del|}(); 
                        }
                    }
                }
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                using System;
                class A
                {
                    static void Del() { }
                    class B
                    {
                        delegate void Del();
                        void Boo()
                        {
                            Del d = new Del(A.Del); 
                            Del(); 
                        }
                    }
                }                
                </text>
            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyNot_Delegate2()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                using System;
                class A
                {
                    static void Bar() { }
                    class B
                    {
                        delegate void Del();
                        void Bar() { }
                        void Boo()
                        {
                            Del d = new Del(A.{|SimplifyParent:Bar|}); 
                            A.{|SimplifyParent:Bar|}();
                        }
                    }
                }
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                using System;
                class A
                {
                    static void Bar() { }
                    class B
                    {
                        delegate void Del();
                        void Bar() { }
                        void Boo()
                        {
                            Del d = new Del(A.Bar); 
                            A.Bar();
                        }
                    }
                }
                </text>
            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyNot_Delegate3()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    using System;
                    class A
                    {
                        delegate void Del(Del a);
                        static void Boo(Del a) { }
                        class B
                        {
                            Del Boo = new Del(A.Boo);
                            void Foo()
                            {
                                Boo(A.{|SimplifyParent:Boo|}); 
                                A.Boo(Boo);
                            }
                        }
                    }
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                    using System;
                    class A
                    {
                        delegate void Del(Del a);
                        static void Boo(Del a) { }
                        class B
                        {
                            Del Boo = new Del(A.Boo);
                            void Foo()
                            {
                                Boo(A.Boo); 
                                A.Boo(Boo);
                            }
                        }
                    }
                </text>
            Test(input, expected)
        End Sub

        <Fact(), Trait(Traits.Feature, Traits.Features.Simplification)>
        <WorkItem(552722)>
        Public Sub TestSimplifyNot_Action()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document><![CDATA[
                using System;
                class A
                {
                    static Action<int> Bar = (int x) => { };    
                    class B
                    {
                        Action<int> Bar = (int x) => { };
                        void Foo()
                        {
                            A.{|SimplifyParent:Bar|}(3);            
                        }
                    }
                }]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
                using System;
                class A
                {
                    static Action<int> Bar = (int x) => { };    
                    class B
                    {
                        Action<int> Bar = (int x) => { };
                        void Foo()
                        {
                            A.Bar(3);            
                        }
                    }
                }]]>
              </text>
            Test(input, expected)
        End Sub

        <Fact(), Trait(Traits.Feature, Traits.Features.Simplification)>
        <WorkItem(552722)>
        Public Sub TestSimplifyNot_Func()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document><![CDATA[
                using System;
                class A
                {
                    static Func<int,int> Bar = (int x) => { return x; };    
                    class B
                    {
                        Func<int,int> Bar = (int x) => { return x; };
                        void Foo()
                        {
                            A.{|SimplifyParent:Bar|}(3);            
                        }
                    }
                }]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
                using System;
                class A
                {
                    static Func<int,int> Bar = (int x) => { return x; };    
                    class B
                    {
                        Func<int,int> Bar = (int x) => { return x; };
                        void Foo()
                        {
                            A.Bar(3);            
                        }
                    }
                }]]>
              </text>
            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyNot_Inheritance()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
using System;
class A
{
    public virtual void f() { }
}
class B : A
{
    public override void f()
    {
        base.{|SimplifyParent:f|}();
    }
}
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
using System;
class A
{
    public virtual void f() { }
}
class B : A
{
    public override void f()
    {
        base.f();
    }
}        
              </text>
            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyDoNothingWithFailedOverloadResolution()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
using System;
using System.Console;
class A
{
    public void f()
    {
        Console.{|SimplifyParent:ReadLine|}("Boo!");
    }
}
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
using System;
using System.Console;
class A
{
    public void f()
    {
        Console.ReadLine("Boo!");
    }
}
              </text>
            Test(input, expected)
        End Sub

        <WorkItem(609496)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub CSharpDoNotSimplifyNameInNamespaceDeclaration()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
using System;
namespace System.{|SimplifyParent:Foo|}
{}
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
using System;
namespace System.Foo
{}
              </text>

            Test(input, expected)
        End Sub

        <WorkItem(608197)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub CS_EscapeAliasReplacementIfNeeded()
            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
            using @if = System.Runtime.InteropServices.InAttribute;
            class C
            {
                void foo()
                {
                    var x = new System.Runtime.InteropServices.{|SimplifyParent:InAttribute|}() // Simplify Type Name
                }
            }
        </Document>
    </Project>
</Workspace>

            Dim expected =
<Code>
            using @if = System.Runtime.InteropServices.InAttribute;
            class C
            {
                void foo()
                {
                    var x = new @if() // Simplify Type Name
                }
            }
</Code>

            Test(input, expected)
        End Sub

        <WorkItem(529989)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub CS_AliasReplacementKeepsUnicodeEscaping()
            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
        using B\u0061r = System.Console;

        class Program
        {
            static void Main(string[] args)
            {
                System.Console.{|SimplifyParent:WriteLine|}("");
            }
        }
        </Document>
    </Project>
</Workspace>

            Dim expected =
<Code>
        using B\u0061r = System.Console;

        class Program
        {
            static void Main(string[] args)
            {
                B\u0061r.WriteLine("");
            }
        }
</Code>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub CSharp_Simplify_Cast_Type_Name()
            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        var a = 1;
        Console.WriteLine((System.Collections.Generic.{|SimplifyParent:IEnumerable&lt;int&gt;|})a);
    }
}
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        var a = 1;
        Console.WriteLine((IEnumerable&lt;int&gt;)a);
    }
}
</code>

            Test(input, expected)

        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestAliasedNameWithMethod()
            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
using System;
using System.Collections.Generic;

using foo = System.Console;
class Program
{
    static void Main(string[] args)
    {
             {|SimplifyExtension:System.Console.WriteLine|}("test");
    }
}
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
using System;
using System.Collections.Generic;

using foo = System.Console;
class Program
{
    static void Main(string[] args)
    {
             foo.WriteLine("test");
    }
}
</code>

            Test(input, expected)

        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        <WorkItem(554010)>
        Public Sub TestSimplificationForDelegateCreation()
            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
using System;
class Test
{
    static void Main(string[] args)
    {
        Action b = (Action)Console.WriteLine + {|SimplifyParent:System.Console.WriteLine|};
    }
}
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
using System;
class Test
{
    static void Main(string[] args)
    {
        Action b = (Action)Console.WriteLine + Console.WriteLine;
    }
}
</code>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        <WorkItem(554010)>
        Public Sub TestSimplificationForDelegateCreation2()
            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
using System;
class Test
{
    Action b = (Action)Console.WriteLine + {|SimplifyParent:System.Console.WriteLine|};
}
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
using System;
class Test
{
    Action b = (Action)Console.WriteLine + Console.WriteLine;
}
</code>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        <WorkItem(576970)>
        Public Sub TestCSRemoveThisWouldBeConsideredACast_1()
            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
using System;
 
class C
{
    Action A { get; set; }
 
    void Foo()
    {
        (this.{|SimplifyParent:A|})(); // Simplify type name
    }
}
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
using System;
 
class C
{
    Action A { get; set; }
 
    void Foo()
    {
        (this.A)(); // Simplify type name
    }
}
</code>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        <WorkItem(576970)>
        Public Sub TestCSRemoveThisWouldBeConsideredACast_2()
            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
using System;
 
class C
{
    Action A { get; set; }
 
    void Foo()
    {
        ((this.{|SimplifyParent:A|}))(); // Simplify type name
    }
}
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
using System;
 
class C
{
    Action A { get; set; }
 
    void Foo()
    {
        ((A))(); // Simplify type name
    }
}
</code>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        <WorkItem(576970)>
        Public Sub TestCSRemoveThisWouldBeConsideredACast_3()
            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
using System;

public class C
{
    public class D
    {
        public Action A { get; set; }
    }

    public D d = new D();

    void Foo()
    {
        (this.{|SimplifyParent:d|}.A)(); // Simplify type name
    }
}
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
using System;

public class C
{
    public class D
    {
        public Action A { get; set; }
    }

    public D d = new D();

    void Foo()
    {
        (this.d.A)(); // Simplify type name
    }
}
</code>

            Test(input, expected)
        End Sub

        <WorkItem(649385)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub CSharpSimplifyToVarCorrect()

            Dim simplificationOption = New Dictionary(Of OptionKey, Object) From {{SimplificationOptions.PreferImplicitTypeInLocalDeclaration, True}}

            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
using System.IO;
using I = N.C;
namespace N { class C {} }
class Program
{
    class D { }
    static void Main(string[] args)
    {
        {|Simplify:int|} i = 0;

        for ({|Simplify:int|} j = 0; ;) { }

        {|Simplify:D|} d = new D();

        foreach ({|Simplify:int|} item in new List&lt;int&gt;()) { }

        using ({|Simplify:StreamReader|} file = new StreamReader("C:\\myfile.txt")) {}

        {|Simplify:int|} x = Foo();
    }
    static int Foo() { return 1; }
}
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
using System.IO;
using I = N.C;
namespace N { class C {} }
class Program
{
    class D { }
    static void Main(string[] args)
    {
        var i = 0;

        for (var j = 0; ;) { }

        var d = new D();

        foreach (var item in new List&lt;int&gt;()) { }

        using (var file = new StreamReader("C:\\myfile.txt")) {}

        var x = Foo();
    }
    static int Foo() { return 1; }
}
 
</code>

            Test(input, expected, simplificationOption)
        End Sub

        <WorkItem(734445)>
        <WorkItem(649385)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub CSharpSimplifyToVarCorrect_QualifiedTypeNames()

            Dim simplificationOption = New Dictionary(Of OptionKey, Object) From {{SimplificationOptions.PreferImplicitTypeInLocalDeclaration, True}}

            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
using System.IO;
namespace N { class C {} }
class Program
{
    class D { }
    static void Main(string[] args)
    {
        {|SimplifyParent:N.C|} z = new N.C();

        {|SimplifyParent:System.Int32|} i = 1;
    }
}
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
using System.IO;
namespace N { class C {} }
class Program
{
    class D { }
    static void Main(string[] args)
    {
        var z = new N.C();

        var i = 1;
    }
}
 
</code>

            Test(input, expected, simplificationOption)
        End Sub

        <WorkItem(649385)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub CSharpSimplifyToVarDontSimplify()

            Dim simplificationOption = New Dictionary(Of OptionKey, Object) From {{SimplificationOptions.PreferImplicitTypeInLocalDeclaration, True}}

            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
using System;

class Program
{
    {|Simplify:int|} x;

    static void Main(string[] args)
    {
        {|Simplify:int|} i = (i = 20);

        {|Simplify:object|} o = null;

        {|Simplify:Action&lt;string[]&gt;|} m = Main;

        {|Simplify:int|} ij = 0, k = 0;

        {|Simplify:int|} j;

        {|Simplify:dynamic|} d = 1;
    }
}
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
using System;

class Program
{
    int x;

    static void Main(string[] args)
    {
        int i = (i = 20);

        object o = null;

        Action&lt;string[]&gt; m = Main;

        int ij = 0, k = 0;

        int j;

        dynamic d = 1;
    }
}
</code>

            Test(input, expected, simplificationOption)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyTypeNameWhenParentHasSimplifyAnnotation()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    using System;
                    namespace Root 
                    {
                        {|SimplifyParent:class A 
                        {
                            System.Exception c;
                        }|}
                    }
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                    using System;
                    namespace Root 
                    {
                        class A 
                        {
                            Exception c;
                        }
                    }
                </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyTypeNameWithExplicitSimplifySpan_MutuallyExclusive()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    {|SpanToSimplify:using System;|}
                    namespace Root 
                    {
                        {|SimplifyParent:class A 
                        {
                            System.Exception c;
                        }|}
                    }
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                    using System;
                    namespace Root 
                    {
                        class A 
                        {
                            System.Exception c;
                        }
                    }
                </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyTypeNameWithExplicitSimplifySpan_Inclusive()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    {|SpanToSimplify:using System;
                    namespace Root 
                    {
                        {|SimplifyParent:class A 
                        {
                            System.Exception c;
                        }|}
                    }
                    |}
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                    using System;
                    namespace Root 
                    {
                        class A 
                        {
                            Exception c;
                        }
                    }
                </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyTypeNameWithExplicitSimplifySpan_OverlappingPositive()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    using System;
                    namespace Root 
                    {
                        {|SimplifyParent:class A 
                        {
                            {|SpanToSimplify:System.Exception|} c;
                        }|}
                    }                    
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                    using System;
                    namespace Root 
                    {
                        class A 
                        {
                            Exception c;
                        }
                    }
                </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyTypeNameWithExplicitSimplifySpan_OverlappingNegative()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    using System;
                    namespace Root 
                    {
                        {|SimplifyParent:class A 
                        {
                            System.Exception {|SpanToSimplify:c;|}
                        }|}
                    }                    
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                    using System;
                    namespace Root 
                    {
                        class A 
                        {
                            System.Exception c;
                        }
                    }
                </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification), WorkItem(864735)>
        Public Sub BugFix864735_CSharp_SimplifyNameInIncompleteIsExpression()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                class C
                {
                    static int F;

                    void M(C other)
                    {
                        Console.WriteLine({|SimplifyParent:C.F|} is
                    }
                }
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                class C
                {
                    static int F;

                    void M(C other)
                    {
                        Console.WriteLine(F is
                    }
                }
                </text>

            Test(input, expected)
        End Sub

        <WorkItem(813566)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyQualifiedCref()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document><![CDATA[
                    using System;

                    /// <summary>
                    /// <see cref="{|Simplify:System.Object|}"/>
                    /// </summary>
                    class Program
                    {
                    }]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text><![CDATA[
                    using System;

                    /// <summary>
                    /// <see cref="object"/>
                    /// </summary>
                    class Program
                    {
                    }]]>
              </text>

            Test(input, expected)
        End Sub

        <WorkItem(838109)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub DontSimplifyToGenericNameCSharp()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document><![CDATA[
public class C<T>
{
    public class D
    {
        public static void F()
        {
        }
    }
}
 
public class C : C<int>
{
}
 
class E
{
    public static void Main()
    {
        {|SimplifyParent:C.D|}.F();
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text><![CDATA[
public class C<T>
{
    public class D
    {
        public static void F()
        {
        }
    }
}
 
public class C : C<int>
{
}
 
class E
{
    public static void Main()
    {
        C.D.F();
    }
}]]>
              </text>

            Test(input, expected)
        End Sub

        <WorkItem(838109)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub DoSimplifyToGenericName()
            Dim simplificationOption = New Dictionary(Of OptionKey, Object) From {{SimplificationOptions.AllowSimplificationToGenericType, True}}

            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document><![CDATA[
public class C<T>
{
    public class D
    {
        public static void F()
        {
        }
    }
}
 
public class C : C<int>
{
}
 
class E
{
    public static void Main()
    {
        {|SimplifyParent:C.D|}.F();
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text><![CDATA[
public class C<T>
{
    public class D
    {
        public static void F()
        {
        }
    }
}
 
public class C : C<int>
{
}
 
class E
{
    public static void Main()
    {
        C<int>.D.F();
    }
}]]>
              </text>

            Test(input, expected, simplificationOption)
        End Sub

        <Fact, WorkItem(838109), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestDontSimplifyAllNodes_SimplifyNestedType()
            Dim simplificationOption = New Dictionary(Of OptionKey, Object) From {{SimplificationOptions.AllowSimplificationToBaseType, False}}

            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
using System;

class Preserve
{
	public class X
	{
		public static int Y;
	}
}

class Z<T> : Preserve
{
}

static class M
{
	public static void Main()
	{
		int k = {|SimplifyParent:Z<float>.X.Y|};
	}
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
using System;

class Preserve
{
	public class X
	{
		public static int Y;
	}
}

class Z<T> : Preserve
{
}

static class M
{
	public static void Main()
	{
		int k = Z<float>.X.Y;
	}
}]]></text>

            Test(input, expected, simplificationOption)
        End Sub

        <Fact, WorkItem(838109), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestDontSimplifyAwayThisExpression()
            Dim simplificationOption = New Dictionary(Of OptionKey, Object) From {{New OptionKey(SimplificationOptions.QualifyMemberAccessWithThisOrMe, LanguageNames.CSharp), True}}

            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
using System;

class Program
{
    private int s;
    static void Main(string[] args)
    {
        
    }

    public int give()
    {
        return {|SimplifyParent:this.s|};
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
using System;

class Program
{
    private int s;
    static void Main(string[] args)
    {
        
    }

    public int give()
    {
        return this.s;
    }
}]]></text>

            Test(input, expected, simplificationOption)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyTypeNameInCodeWithSyntaxErrors()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
class C
{
    private int x;

    void F(int y) {}

    void M()
    {
        C
        // some comment
        F({|SimplifyParent:this.x|});
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
class C
{
    private int x;

    void F(int y) {}

    void M()
    {
        C
        // some comment
        F(x);
    }
}]]></text>

            Test(input, expected)
        End Sub
        <Fact, WorkItem(653601), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestCrefSimplification_1()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
namespace A
{
    /// <summary>
    /// <see cref="{|Simplify:A.Program|}"/>
    /// </summary>
    class Program
    {
        void B()
        {

        }
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
namespace A
{
    /// <summary>
    /// <see cref="Program"/>
    /// </summary>
    class Program
    {
        void B()
        {

        }
    }
}]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(653601), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestCrefSimplification_2()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
namespace A
{
    /// <summary>
    /// <see cref="{|Simplify:A.Program.B|}"/>
    /// </summary>
    class Program
    {
        void B()
        {

        }
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
namespace A
{
    /// <summary>
    /// <see cref="B"/>
    /// </summary>
    class Program
    {
        void B()
        {

        }
    }
}]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(966633), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub CSharp_DontSimplifyNullableQualifiedName()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
using System;
class C
{
       {|SimplifyParent:Nullable<long>|}.Value x;
       void M({|SimplifyParent:Nullable<int>|} a, ref {|SimplifyParent:System.Nullable<int>|} b, ref {|SimplifyParent:Nullable<long>|}.Something c) { }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
using System;
class C
{
       Nullable<long>.Value x;
       void M(int? a, ref int? b, ref Nullable<long>.Something c) { }
}]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(965240), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub CSharp_DontSimplifyOpenGenericNullable()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
using System;
class C
{
    void M()
    {
        var x = typeof({|SimplifyParent:System.Nullable<>|});
        var y = (typeof({|SimplifyParent:System.Nullable<long>|}));
        var z = (typeof({|SimplifyParent:System.Nullable|}));
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
using System;
class C
{
    void M()
    {
        var x = typeof(Nullable<>);
        var y = (typeof(long?));
        var z = (typeof(Nullable));
    }
}]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(1067214), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub CSharp_SimplifyTypeNameInExpressionBody_Property()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
namespace N
{
    class Program
    {
        private object x;
        public Program X => ({|SimplifyParent:N.Program|})x;
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
namespace N
{
    class Program
    {
        private object x;
        public Program X => (Program)x;
    }
}]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(1067214), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub CSharp_SimplifyTypeNameInExpressionBody_Method()
            Dim input =
        <Workspace>
            <Project Language="C#" CommonReferences="true">
                <Document>
                    <![CDATA[
namespace N
{
    class Program
    {
        private object x;
        public Program X() => ({|SimplifyParent:N.Program|})x;
    }
}]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
namespace N
{
    class Program
    {
        private object x;
        public Program X() => (Program)x;
    }
}]]></text>

            Test(input, expected)
        End Sub
#End Region

#Region "Normal Visual Basic Tests"

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyAllNodes_SimplifyTypeName()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                    Imports System
                      Namespace Root
                        Class A
                            Private e As {|SimplifyParent:System.Exception|}
                        End Class
                    End Namespace
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                      Imports System
                      Namespace Root
                        Class A
                            Private e As Exception
                        End Class
                    End Namespace
                </text>

            Test(input, expected)
        End Sub

        <WorkItem(547117)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestGetChanges_SimplifyTypeName_Array_1()
            Dim input =
                    <Workspace>
                        <Project Language="Visual Basic" CommonReferences="true">
                            <Document>
                    Module Program
                    Dim Foo() As Integer

                    Sub Main(args As String())
                        {|SimplifyParent:Program|}.Foo(23) = 23
                    End Sub
                    End Module
                </Document>
                        </Project>
                    </Workspace>

            Dim expected =
              <text>
                    Module Program
                    Dim Foo() As Integer

                    Sub Main(args As String())
                        Foo(23) = 23
                    End Sub
                    End Module
                </text>

            Test(input, expected)
        End Sub

        <WorkItem(547117)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestGetChanges_SimplifyTypeName_Array_2()
            Dim input =
                    <Workspace>
                        <Project Language="Visual Basic" CommonReferences="true">
                            <Document>
Module Program
    Dim Bar() As Action(Of Integer)

    Sub Main(args As String())
        {|SimplifyParent:Program.Bar|}(2)(2)
    End Sub
End Module
                </Document>
                        </Project>
                    </Workspace>

            Dim expected =
              <text>
Module Program
    Dim Bar() As Action(Of Integer)

    Sub Main(args As String())
        Bar(2)(2)
    End Sub
End Module
                </text>

            Test(input, expected)
        End Sub

        <WorkItem(547117)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestGetChanges_SimplifyTypeName_Receiver1()
            Dim input =
                    <Workspace>
                        <Project Language="Visual Basic" CommonReferences="true">
                            <Document>
Class C
    Sub M(other As C)
        {|SimplifyParent:other.M|}(Nothing)
    End Sub
End Class
                </Document>
                        </Project>
                    </Workspace>

            Dim expected =
              <text>
Class C
    Sub M(other As C)
        other.M(Nothing)
    End Sub
End Class

                </text>

            Test(input, expected)
        End Sub

        <WorkItem(547117)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestGetChanges_SimplifyTypeName_Receiver2()
            Dim input =
                    <Workspace>
                        <Project Language="Visual Basic" CommonReferences="true">
                            <Document>
Class C
    Sub M(other As C)
        {|SimplifyParent:Me.M|}(Nothing)
    End Sub
End Class
                </Document>
                        </Project>
                    </Workspace>

            Dim expected =
              <text>
Class C
    Sub M(other As C)
        M(Nothing)
    End Sub
End Class
                </text>

            Test(input, expected)
        End Sub

        <WorkItem(547117)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestGetChanges_SimplifyTypeName_Receiver3()
            Dim input =
                    <Workspace>
                        <Project Language="Visual Basic" CommonReferences="true">
                            <Document>
Class A
    Public Shared B As Action

    Public Sub M(ab As A)
        {|SimplifyParent:ab.B|}()
    End Sub
End Class
                </Document>
                        </Project>
                    </Workspace>

            Dim expected =
              <text>
Class A
    Public Shared B As Action

    Public Sub M(ab As A)
        ab.B()
    End Sub
End Class
                </text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyAllNodes_SimplifyNestedType()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                    <![CDATA[
Class Preserve
	Public Class X
		Public Shared Y
	End Class
End Class

Class Z(Of T)
	Inherits Preserve
End Class

NotInheritable Class M
	Public Shared Sub Main()
        ReDim {|SimplifyParent:Z(Of Integer).X.Y(1)|}
	End Sub
End Class]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
Class Preserve
	Public Class X
		Public Shared Y
	End Class
End Class

Class Z(Of T)
	Inherits Preserve
End Class

NotInheritable Class M
	Public Shared Sub Main()
        ReDim [Preserve].X.Y(1)
	End Sub
End Class]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyAllNodes_SimplifyNestedType2()
            ' Simplified type is in a different namespace.

            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
Namespace N1
	Class Preserve
		Public Class X
			Public Shared Y
		End Class
	End Class
End Namespace

Namespace P
	Class NonGeneric
		Inherits N1.Preserve
	End Class
End Namespace

NotInheritable Class M
	Public Shared Sub Main()
		ReDim P.NonGeneric.{|SimplifyParent:X|}.Y(1)
	End Sub
End Class
               </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
Namespace N1
	Class Preserve
		Public Class X
			Public Shared Y
		End Class
	End Class
End Namespace

Namespace P
	Class NonGeneric
		Inherits N1.Preserve
	End Class
End Namespace

NotInheritable Class M
	Public Shared Sub Main()
		ReDim N1.Preserve.X.Y(1)
	End Sub
End Class</text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyAllNodes_SimplifyNestedType3()
            ' Simplified type is in a different namespace, whose names have been imported with an Imports statement.

            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
Imports N1

Namespace N1
	Class Preserve
		Public Class X
			Public Shared Y
		End Class
	End Class
End Namespace

Namespace P
	Class NonGeneric
		Inherits N1.Preserve
	End Class
End Namespace

Namespace R
	NotInheritable Class M
		Public Shared Sub Main()
			Redim P.NonGeneric.{|SimplifyParent:X|}.Y(1)
		End Sub
	End Class
End Namespace
              </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
Imports N1

Namespace N1
	Class Preserve
		Public Class X
			Public Shared Y
		End Class
	End Class
End Namespace

Namespace P
	Class NonGeneric
		Inherits N1.Preserve
	End Class
End Namespace

Namespace R
	NotInheritable Class M
		Public Shared Sub Main()
			Redim [Preserve].X.Y(1)
		End Sub
	End Class
End Namespace</text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyAllNodes_SimplifyNestedType4()
            ' Highly nested type simplified to another highly nested type.

            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
Imports P1.P2
Imports Q1.Q2

Namespace N1
	Namespace N2
		Public Class Outer
			Public Class Preserve
				Public Class X
					Public Shared Y As Integer
				End Class
			End Class
		End Class
	End Namespace
End Namespace

Namespace P1
	Namespace P2
		Public Class NonGeneric
			Inherits N1.N2.Outer.Preserve
		End Class
	End Namespace
End Namespace

Namespace Q1
	Namespace Q2
		Class Generic(Of T)
			Inherits NonGeneric
		End Class
	End Namespace
End Namespace

Namespace R
	NotInheritable Class M
		Public Shared Sub Main()
			Dim k As Integer = Generic(Of Integer).{|SimplifyParent:X|}.Y
		End Sub
	End Class
End Namespace
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  Imports P1.P2
Imports Q1.Q2

Namespace N1
	Namespace N2
		Public Class Outer
			Public Class Preserve
				Public Class X
					Public Shared Y As Integer
				End Class
			End Class
		End Class
	End Namespace
End Namespace

Namespace P1
	Namespace P2
		Public Class NonGeneric
			Inherits N1.N2.Outer.Preserve
		End Class
	End Namespace
End Namespace

Namespace Q1
	Namespace Q2
		Class Generic(Of T)
			Inherits NonGeneric
		End Class
	End Namespace
End Namespace

Namespace R
	NotInheritable Class M
		Public Shared Sub Main()
			Dim k As Integer = N1.N2.Outer.Preserve.X.Y
		End Sub
	End Class
End Namespace</text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyAllNodes_SimplifyNestedType5()
            ' Name requiring multiple iterations of nested type simplification.

            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
Imports N1.N2
Imports P1.P2
Imports Q1.Q2

Namespace N1
	Namespace N2
		Public Class Outer
			Public Class Preserve
				Public Class X
					Public Shared Y As Integer
				End Class
			End Class
		End Class
	End Namespace
End Namespace

Namespace P1
	Namespace P2
		Public Class NonGeneric
			Inherits Outer
			Public Class NonGenericInner
				Inherits Outer.Preserve
			End Class
		End Class
	End Namespace
End Namespace

Namespace Q1
	Namespace Q2
		Class Generic(Of T)
			Inherits NonGeneric
		End Class
	End Namespace
End Namespace

Namespace R
	NotInheritable Class M
		Public Shared Sub Main()
			Dim k As Integer = Generic(Of Integer).NonGenericInner.{|SimplifyParent:X|}.Y
		End Sub
	End Class
End Namespace
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
Imports N1.N2
Imports P1.P2
Imports Q1.Q2

Namespace N1
	Namespace N2
		Public Class Outer
			Public Class Preserve
				Public Class X
					Public Shared Y As Integer
				End Class
			End Class
		End Class
	End Namespace
End Namespace

Namespace P1
	Namespace P2
		Public Class NonGeneric
			Inherits Outer
			Public Class NonGenericInner
				Inherits Outer.Preserve
			End Class
		End Class
	End Namespace
End Namespace

Namespace Q1
	Namespace Q2
		Class Generic(Of T)
			Inherits NonGeneric
		End Class
	End Namespace
End Namespace

Namespace R
	NotInheritable Class M
		Public Shared Sub Main()
			Dim k As Integer = Outer.Preserve.X.Y
		End Sub
	End Class
End Namespace</text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyAllNodes_SimplifyStaticMemberAccess()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
Class Preserve
	Public Shared Y
End Class

Class Z(Of T)
	Inherits Preserve
End Class

NotInheritable Class M
	Public Shared Sub Main()
		Redim {|SimplifyParent:Z(Of Single).Y(1)|}
	End Sub
End Class
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
Class Preserve
	Public Shared Y
End Class

Class Z(Of T)
	Inherits Preserve
End Class

NotInheritable Class M
	Public Shared Sub Main()
		Redim [Preserve].Y(1)
	End Sub
End Class</text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyAllNodes_SimplifyQualifiedName()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
Class A
	Public NotInheritable Class B
	End Class
End Class

Class C
	Inherits A
End Class

Namespace N1
	NotInheritable Class M
		Public Shared Function F() As {|SimplifyParent:C.B|}
			Return Nothing
		End Function
	End Class
End Namespace
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
Class A
	Public NotInheritable Class B
	End Class
End Class

Class C
	Inherits A
End Class

Namespace N1
	NotInheritable Class M
		Public Shared Function F() As A.B
			Return Nothing
		End Function
	End Class
End Namespace</text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(551040), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyAllNodes_SimplifyAliasStaticMemberAccess()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
Imports X = NonGeneric

Class Preserve
	Public Shared Y
End Class

Class NonGeneric
	Inherits Preserve
End Class

Namespace N1
	NotInheritable Class M
		Public Shared Sub Main()
			Redim {|SimplifyParent:X.Y(1)|}
		End Sub
	End Class
End Namespace
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
Imports X = NonGeneric

Class Preserve
	Public Shared Y
End Class

Class NonGeneric
	Inherits Preserve
End Class

Namespace N1
	NotInheritable Class M
		Public Shared Sub Main()
			Redim [Preserve].Y(1)
		End Sub
	End Class
End Namespace</text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyNot_Delegate1_VB()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                Class A
                    Shared Sub Del()
                    End Sub
                    Class B
                        Delegate Sub Del()
                        Sub Boo()
                            Dim d As Del = New Del(AddressOf A.{|SimplifyParent:Del|})
                            A.{|SimplifyParent:Del|}()
                        End Sub
                    End Class
                End Class      
              </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                Class A
                    Shared Sub Del()
                    End Sub
                    Class B
                        Delegate Sub Del()
                        Sub Boo()
                            Dim d As Del = New Del(AddressOf A.Del)
                            A.Del()
                        End Sub
                    End Class
                End Class           
                </text>
            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyNot_Delegate2_VB()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                Class A
                    Shared Sub Bar()
                    End Sub
                    Class B
                        Delegate Sub Del()
                        Sub Bar()
                        End Sub
                        Sub Boo()
                            Dim d As Del = New Del(AddressOf A.{|SimplifyParent:Bar|})
                        End Sub
                    End Class
                End Class
              </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                Class A
                    Shared Sub Bar()
                    End Sub
                    Class B
                        Delegate Sub Del()
                        Sub Bar()
                        End Sub
                        Sub Boo()
                            Dim d As Del = New Del(AddressOf A.Bar)
                        End Sub
                    End Class
                End Class
                </text>
            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        <WorkItem(570986)>
        <WorkItem(552722)>
        Public Sub TestSimplifyNot_Action_VB()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                Imports System

                Class A
                    Shared Bar As Action(Of Integer) = New Action(Of Integer)(Function(x) x + 1)
                    Class B
                        Shared Bar As Action(Of Integer) = New Action(Of Integer)(Function(x) x + 1)
                        Sub Foo()
                            A.{|SimplifyParent:Bar|}(3)
                        End Sub
                    End Class
                End Class
              </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                Imports System

                Class A
                    Shared Bar As Action(Of Integer) = New Action(Of Integer)(Function(x) x + 1)
                    Class B
                        Shared Bar As Action(Of Integer) = New Action(Of Integer)(Function(x) x + 1)
                        Sub Foo()
                            A.Bar(3)
                        End Sub
                    End Class
                End Class
                </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestSimplifyBaseInheritanceVB()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                MustInherit Class A
                    Public MustOverride Sub Foo()
                    Public Sub Boo()
                    End Sub
                End Class
                Class B
                    Inherits A
                    Public Overrides Sub Foo()
                        MyBase.{|SimplifyParent:Boo|}()
                    End Sub
                End Class    
              </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                MustInherit Class A
                    Public MustOverride Sub Foo()
                    Public Sub Boo()
                    End Sub
                End Class
                Class B
                    Inherits A
                    Public Overrides Sub Foo()
                        Boo()
                    End Sub
                End Class    
                </text>
            Test(input, expected)
        End Sub

        <WorkItem(588099)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_EscapeReservedNamesInAttributes()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" CommonReferences="true">
        <Document>
Imports System

&lt;Global.Assembly.{|SimplifyParent:Foo|}&gt;
Module Assembly
    Class FooAttribute
        Inherits Attribute
    End Class
End Module

Module M
    Class FooAttribute
        Inherits Attribute
    End Class
End Module
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
Imports System

&lt;[Assembly].Foo&gt;
Module Assembly
    Class FooAttribute
        Inherits Attribute
    End Class
End Module

Module M
    Class FooAttribute
        Inherits Attribute
    End Class
End Module
</code>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_OmitModuleNameInMemberAccess()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" CommonReferences="true">
        <Document>
Imports System

Namespace foo
    Module Program
        Sub Main(args As String())

        End Sub
    End Module
End Namespace

Namespace bar
    Module b
        Sub m()
            foo.Program.{|SimplifyParent:Main|}(Nothing)
        End Sub
    End Module
End Namespace
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
Imports System

Namespace foo
    Module Program
        Sub Main(args As String())

        End Sub
    End Module
End Namespace

Namespace bar
    Module b
        Sub m()
            foo.Main(Nothing)
        End Sub
    End Module
End Namespace
</code>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_OmitModuleNameInQualifiedName()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" CommonReferences="true">
        <Document>
Imports System

Namespace foo
    Module Program
        Sub Main(args As String())
        End Sub
        Class C1
        End Class
    End Module
End Namespace

Namespace bar
    Module b
        Sub m()
            Dim x as foo.Program.{|SimplifyParent:C1|}
        End Sub
    End Module
End Namespace
        </Document>
    </Project>
</Workspace>

            Dim expected =
<code>
Imports System

Namespace foo
    Module Program
        Sub Main(args As String())
        End Sub
        Class C1
        End Class
    End Module
End Namespace

Namespace bar
    Module b
        Sub m()
            Dim x as foo.C1
        End Sub
    End Module
End Namespace
</code>

            Test(input, expected)
        End Sub

        <WorkItem(601160)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub TestExpandMultilineLambdaWithImports()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" CommonReferences="true">
        <Document>
Module Program
    {|Expand:Sub Main(args As String())
        Task.Run(Sub() 
Imports System 
End Sub)
    End Sub|}
End Module
        </Document>
    </Project>
</Workspace>
            Using workspace = TestWorkspaceFactory.CreateWorkspace(input)
                Dim hostDocument = workspace.Documents.Single()
                Dim document = workspace.CurrentSolution.Projects.Single().Documents.Single()
                Dim root = document.GetSyntaxRootAsync().Result

                For Each span In hostDocument.AnnotatedSpans("Expand")
                    Dim node = root.FindToken(span.Start).Parent.Parent
                    If TypeOf node Is MethodBlockBaseSyntax Then
                        node = DirectCast(node, MethodBlockBaseSyntax).Statements.Single()
                    End If

                    Assert.True(TypeOf node Is ExpressionStatementSyntax)

                    Dim result = Simplifier.ExpandAsync(node, document).Result

                    Assert.NotEqual(0, result.ToString().Count)
                Next
            End Using
        End Sub

        <WorkItem(609496)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VB_DoNotReduceNamesInNamespaceDeclarations()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" CommonReferences="true">
        <Document>
            Imports System
            Namespace System.{|SimplifyParent:Foo|}
            End Namespace
        </Document>
    </Project>
</Workspace>

            Dim expected =
<Code>
            Imports System
            Namespace System.Foo
            End Namespace
</Code>

            Test(input, expected)
        End Sub

        <WorkItem(608197)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VB_EscapeAliasReplacementIfNeeded()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" CommonReferences="true">
        <Document>
            Imports [In] = System.Runtime.InteropServices.InAttribute
            Module M
                Dim x = New System.Runtime.InteropServices.{|SimplifyParent:InAttribute|}() ' Simplify Type Name
            End Module
        </Document>
    </Project>
</Workspace>

            Dim expected =
<Code>
            Imports [In] = System.Runtime.InteropServices.InAttribute
            Module M
                Dim x = New [In]() ' Simplify Type Name
            End Module
</Code>

            Test(input, expected)
        End Sub

        <WorkItem(608197)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VB_NoNREForOmittedReceiverInWithBlock()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" CommonReferences="true">
        <Document>
            Public Class A
                Public Sub M()
                    With New B()
                        Dim x = .P.{|SimplifyParent:MaxValue|}
                    End With
                End Sub
            End Class

            Public Class B
                Public Property P As Integer
            End Class
        </Document>
    </Project>
</Workspace>

            Dim expected =
<Code>
            Public Class A
                Public Sub M()
                    With New B()
                        Dim x = .P.MaxValue
                    End With
                End Sub
            End Class

            Public Class B
                Public Property P As Integer
            End Class
</Code>

            Test(input, expected)
        End Sub

        <WorkItem(639971)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub BugFix639971_VisualBasic_FalseUnnecessaryBaseQualifier()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" CommonReferences="true">
        <Document>
Class Z
    Public Function Bar() As String
        Return MyBase.{|SimplifyParent:ToString|}()
    End Function
End Class

Class Y
    Inherits Z
    Public Overrides Function ToString() As String
        Return ""
    End Function

    Public Sub Baz()
        Console.WriteLine(New Y().Bar())
    End Sub
End Class
        </Document>
    </Project>
</Workspace>

            Dim expected =
<Code>
Class Z
    Public Function Bar() As String
        Return MyBase.ToString()
    End Function
End Class

Class Y
    Inherits Z
    Public Overrides Function ToString() As String
        Return ""
    End Function

    Public Sub Baz()
        Console.WriteLine(New Y().Bar())
    End Sub
End Class
</Code>

            Test(input, expected)
        End Sub

        <WorkItem(639971)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub BugFix639971_CSharp_FalseUnnecessaryBaseQualifier()
            Dim input =
<Workspace>
    <Project Language="C#" CommonReferences="true">
        <Document>
using System;

class C
{
    public string InvokeBaseToString()
    {
        return base.{|SimplifyParent:ToString|}();
    }
}

class D : C
{
    public override string ToString()
    {
        return "";
    }

    static void Main()
    {
        Console.WriteLine(new D().InvokeBaseToString());
    }
}
        </Document>
    </Project>
</Workspace>

            Dim expected =
<Code>
using System;

class C
{
    public string InvokeBaseToString()
    {
        return base.ToString();
    }
}

class D : C
{
    public override string ToString()
    {
        return "";
    }

    static void Main()
    {
        Console.WriteLine(new D().InvokeBaseToString());
    }
}
</Code>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyTypeNameWhenParentHasSimplifyAnnotation()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
Imports System
Namespace Root
    {|SimplifyParent:Class A
        Dim c As System.Exception
    End Class|}
End Namespace
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
Imports System
Namespace Root
    Class A
        Dim c As Exception
    End Class
End Namespace
                </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyTypeNameWithExplicitSimplifySpan_MutuallyExclusive()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
{|SpanToSimplify:Imports System|}
Namespace Root
    {|SimplifyParent:Class A
        Dim c As System.Exception
    End Class|}
End Namespace
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
Imports System
Namespace Root
    Class A
        Dim c As System.Exception
    End Class
End Namespace
                </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyTypeNameWithExplicitSimplifySpan_Inclusive()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
{|SpanToSimplify:Imports System
Namespace Root
    {|SimplifyParent:Class A
        Dim c As System.Exception
    End Class|}
End Namespace
|}
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
Imports System
Namespace Root
    Class A
        Dim c As Exception
    End Class
End Namespace
                </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyTypeNameWithExplicitSimplifySpan_OverlappingPositive()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
Imports System
Namespace Root
    {|SimplifyParent:Class A
        Dim c As {|SpanToSimplify:System.Exception|}
    End Class|}
End Namespace
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
Imports System
Namespace Root
    Class A
        Dim c As Exception
    End Class
End Namespace
                </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyTypeNameWithExplicitSimplifySpan_OverlappingNegative()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
Imports System
Namespace Root
    {|SimplifyParent:Class A
        {|SpanToSimplify:Dim c|} As System.Exception
    End Class|}
End Namespace
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
Imports System
Namespace Root
    Class A
        Dim c As System.Exception
    End Class
End Namespace
                </text>

            Test(input, expected)
        End Sub

        <WorkItem(769354)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyTypeNameInCrefCausesConflict()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document><![CDATA[
Imports System

Class Base
    Public Sub New(x As Integer)
    End Sub
End Class
Class Derived : Inherits Base
    ''' <summary>
    ''' <see cref="{|SimplifyParent:Global.Base|}.New(Integer)"/>
    ''' </summary>
    ''' <param name="x"></param>
    Public Sub New(x As Integer)
        MyBase.New(x)
    End Sub
    Public Sub Base(x As Integer)
    End Sub
End Class]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text><![CDATA[
Imports System

Class Base
    Public Sub New(x As Integer)
    End Sub
End Class
Class Derived : Inherits Base
    ''' <summary>
    ''' <see cref="Base.New(Integer)"/>
    ''' </summary>
    ''' <param name="x"></param>
    Public Sub New(x As Integer)
        MyBase.New(x)
    End Sub
    Public Sub Base(x As Integer)
    End Sub
End Class]]>
              </text>

            Test(input, expected)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Simplification), WorkItem(864735)>
        Public Sub BugFix864735_VisualBasic_SimplifyNameInIncompleteIsExpression()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
Imports System
Class C
    Shared Public F As Integer
    Shared Sub Main()
        Console.WriteLine(TypeOf {|SimplifyParent:C.F|} Is
    End Sub
End Class
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
Imports System
Class C
    Shared Public F As Integer
    Shared Sub Main()
        Console.WriteLine(TypeOf F Is
    End Sub
End Class
                </text>

            Test(input, expected)
        End Sub

        <WorkItem(813566)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestSimplifyQualifiedCref()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document><![CDATA[
Imports System

''' <summary>
''' <see cref="{|Simplify:System.Object|}"/>
''' </summary>
Class Program
End Class]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text><![CDATA[
Imports System

''' <summary>
''' <see cref="Object"/>
''' </summary>
Class Program
End Class]]>
              </text>

            Test(input, expected)
        End Sub

        <WorkItem(838109)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_DontSimplifyToGenericName()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document><![CDATA[
Imports System

Class Program
    Sub Main(args As String())
        {|SimplifyParent:C.D|}.F()
    End Sub
End Class
Public Class C(Of T)
    Public Class D
        Public Shared Sub F()
        End Sub
    End Class
End Class
Public Class C
    Inherits C(Of Integer)
End Class
]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text><![CDATA[
Imports System

Class Program
    Sub Main(args As String())
        C.D.F()
    End Sub
End Class
Public Class C(Of T)
    Public Class D
        Public Shared Sub F()
        End Sub
    End Class
End Class
Public Class C
    Inherits C(Of Integer)
End Class
]]>
              </text>

            Test(input, expected)
        End Sub

        <WorkItem(838109)>
        <Fact, Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_DoSimplifyToGenericName()
            Dim simplificationOption = New Dictionary(Of OptionKey, Object) From {{SimplificationOptions.AllowSimplificationToGenericType, True}}

            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document><![CDATA[
Imports System

Class Program
    Sub Main(args As String())
        {|SimplifyParent:C.D|}.F()
    End Sub
End Class
Public Class C(Of T)
    Public Class D
        Public Shared Sub F()
        End Sub
    End Class
End Class
Public Class C
    Inherits C(Of Integer)
End Class
]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text><![CDATA[
Imports System

Class Program
    Sub Main(args As String())
        C(OfInteger).D.F()
    End Sub
End Class
Public Class C(Of T)
    Public Class D
        Public Shared Sub F()
        End Sub
    End Class
End Class
Public Class C
    Inherits C(Of Integer)
End Class
]]>
              </text>

            Test(input, expected, simplificationOption)
        End Sub

        <Fact, WorkItem(838109), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestDontSimplifyAllNodes_SimplifyNestedType()
            Dim simplificationOption = New Dictionary(Of OptionKey, Object) From {{SimplificationOptions.AllowSimplificationToBaseType, False}}

            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                    <![CDATA[
Class Preserve
	Public Class X
		Public Shared Y
	End Class
End Class

Class Z(Of T)
	Inherits Preserve
End Class

NotInheritable Class M
	Public Shared Sub Main()
        ReDim {|SimplifyParent:Z(Of Integer).X.Y(1)|}
	End Sub
End Class]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
Class Preserve
	Public Class X
		Public Shared Y
	End Class
End Class

Class Z(Of T)
	Inherits Preserve
End Class

NotInheritable Class M
	Public Shared Sub Main()
        ReDim Z(Of Integer).X.Y(1)
	End Sub
End Class]]></text>

            Test(input, expected, simplificationOption)
        End Sub

        <Fact, WorkItem(838109), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_TestDontSimplifyAwayMeExpression()
            Dim simplificationOption = New Dictionary(Of OptionKey, Object) From {{New OptionKey(SimplificationOptions.QualifyMemberAccessWithThisOrMe, LanguageNames.VisualBasic), True}}

            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                    <![CDATA[
Imports System
Class Program
    Private s As Integer
    Shared Sub Main(args As String())
    End Sub
    Public Function give() As Integer
        Return {|SimplifyParent:Me.s|}
    End Function
End Class]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
Imports System
Class Program
    Private s As Integer
    Shared Sub Main(args As String())
    End Sub
    Public Function give() As Integer
        Return Me.s
    End Function
End Class]]></text>

            Test(input, expected, simplificationOption)
        End Sub

        <Fact, WorkItem(881746), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_SimplyToAlias()

            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                    <![CDATA[
Imports System
Imports AttributeAttributeAttribute = AttributeAttribute
Class AttributeAttribute
    Inherits Attribute
End Class
 
<{|SimplifyParent:Attribute|}>
Class A
End Class]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
Imports System
Imports AttributeAttributeAttribute = AttributeAttribute
Class AttributeAttribute
    Inherits Attribute
End Class
 
<AttributeAttributeAttribute>
Class A
End Class]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(881746), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_DontSimplifyAlias()

            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                    <![CDATA[
Imports System
Imports AttributeAttributeAttribute = AttributeAttribute
Class AttributeAttribute
    Inherits Attribute
End Class
 
<{|SimplifyParent:AttributeAttributeAttribute|}>
Class A
End Class]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
Imports System
Imports AttributeAttributeAttribute = AttributeAttribute
Class AttributeAttribute
    Inherits Attribute
End Class
 
<AttributeAttributeAttribute>
Class A
End Class]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(966633), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_DontSimplifyNullableQualifiedName()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                    <![CDATA[
Imports System
Module Module1
    Sub Main()
        Dim x as {|SimplifyParent:Nullable(Of Integer)|}.Value
    End Sub
    Sub M(a as {|SimplifyParent:Nullable(Of Integer)|}, byref b as {|SimplifyParent:System.Nullable(Of Integer)|}, byref c as {|SimplifyParent:Nullable(Of Integer)|}.Value)
    End Sub
End Module]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
Imports System
Module Module1
    Sub Main()
        Dim x as Nullable(Of Integer).Value
    End Sub
    Sub M(a as Integer?, byref b as Integer?, byref c as Nullable(Of Integer).Value)
    End Sub
End Module]]></text>

            Test(input, expected)
        End Sub

        <Fact, WorkItem(965240), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_DontSimplifyOpenGenericNullable()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                    <![CDATA[
Imports System
Module Module1
    Sub Main()
        Dim x = GetType({|SimplifyParent:System.Nullable(Of )|})
        Dim y = (GetType({|SimplifyParent:System.Nullable(Of Long)|}))
        Dim z = (GetType({|SimplifyParent:System.Nullable|}))
    End Sub
End Module]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
Imports System
Module Module1
    Sub Main()
        Dim x = GetType(Nullable(Of ))
        Dim y = (GetType(Long?))
        Dim z = (GetType(Nullable))
    End Sub
End Module]]></text>

            Test(input, expected)
        End Sub

        <Fact(Skip:="1019361"), WorkItem(1019361), Trait(Traits.Feature, Traits.Features.Simplification)>
        Public Sub VisualBasic_Bug1019361()
            Dim input =
        <Workspace>
            <Project Language="Visual Basic" CommonReferences="true">
                <Document>
                    <![CDATA[
Imports N
Namespace N
    Class A
        Public Const X As Integer = 1
    End Class
End Namespace

Module Program
    Sub Main()
        Dim x = {|SimplifyParent:N.A|}.X ' Simplify type name 'N.A' 
        Dim a As A = Nothing
    End Sub
End Module]]>
                </Document>
            </Project>
        </Workspace>

            Dim expected =
              <text>
                  <![CDATA[
Imports N
Namespace N
    Class A
        Public Const X As Integer = 1
    End Class
End Namespace

Module Program
    Sub Main()
        Dim x = N.A.X ' Simplify type name 'N.A' 
        Dim a As A = Nothing
    End Sub
End Module]]></text>

            Test(input, expected)
        End Sub
#End Region

    End Class
End Namespace
