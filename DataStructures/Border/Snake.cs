
using System.Collections.Generic;

public class Snake<TNode> : Chain<Segment<TNode>, TNode>
{
    
    //cannot have nodes touching mroe than two others even
    //if not part of same segment
    protected Snake(List<Segment<TNode>> segments) : base(segments)
    {
    }
}
