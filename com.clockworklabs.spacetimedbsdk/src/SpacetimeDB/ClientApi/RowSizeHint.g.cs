// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN YOUR MODULE SOURCE CODE INSTEAD.

#nullable enable

using System;

namespace SpacetimeDB.ClientApi
{
    [SpacetimeDB.Type]
    public partial record RowSizeHint : SpacetimeDB.TaggedEnum<(
        ushort FixedSize,
        System.Collections.Generic.List<ulong> RowOffsets
    )>;
}
