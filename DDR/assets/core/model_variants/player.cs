using DDR;

var models = new Dictionary<string, Model>();
models["idle"] = new ModelBuilder()
    .Vertex3(0, 0, 0) // 0
    .Vertex3(1, 0, 0) // 1
    .Vertex3(1, 1, 0) // 2
    .Vertex3(0, 1, 0) // 3
    .Vertex3(0, 0, 1) // 4
    .Vertex3(1, 0, 1) // 5
    .Vertex3(1, 1, 1) // 6
    .Vertex3(0, 1, 1)
        
    .Index(0).Index(1).Index(2)
    .Index(0).Index(2).Index(3)

    .Index(4).Index(5).Index(6)
    .Index(4).Index(6).Index(7)

    .Index(0).Index(1).Index(5)
    .Index(0).Index(5).Index(4)

    .Index(2).Index(3).Index(7)
    .Index(2).Index(7).Index(6)

    .Index(0).Index(3).Index(7)
    .Index(0).Index(7).Index(4)

    .Index(1).Index(2).Index(6)
    .Index(1).Index(6).Index(5)
    .Build();
models["move"] = new ModelBuilder()
    .Vertex3(0, 0, 0)
    .Vertex3(1, 1, 1)
    .Index(0)
    .Index(1)
    .Build();
return models;