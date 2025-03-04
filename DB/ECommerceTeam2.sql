CREATE DATABASE	ECommerceDB;

USE ECommerceDB;


CREATE TABLE Users(
	id_user UNIQUEIDENTIFIER PRIMARY KEY,
    email NVARCHAR(100) NOT NULL UNIQUE,
    user_role NVARCHAR(50)  NOT NULL,
	CONSTRAINT CK_ruolo CHECK (user_role in ('Seller','User') )
)

CREATE TABLE Sizes (
    id_size INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(50) NOT NULL UNIQUE,
);

CREATE TABLE Colors (
    id_color INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(50) NOT NULL UNIQUE
);


CREATE TABLE Materials (
    id_material INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Category (
    id_category INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Products (
	id_prod UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	brand NVARCHAR (50) NOT NULL,
	gender NVARCHAR(1) NOT NULL,
	nome NVARCHAR (255) NOT NULL,
	id_category INT NOT NULL,
	descr NVARCHAR (1000) NULL,
	price DECIMAL (10,2) NOT NULL,
	stock INT NOT NULL,
	CONSTRAINT CK_presso CHECK (price >0),
	CONSTRAINT CK_stock CHECK (stock >=0),
	CONSTRAINT CK_gender CHECK (gender IN ('F','M','U')),
	CONSTRAINT FK_Prod_Catogry FOREIGN KEY (id_category) REFERENCES Category (id_category),
)


CREATE TABLE ProdSize (
	id_prod UNIQUEIDENTIFIER NOT NULL,
	id_size INT NOT NULL,
	CONSTRAINT PK_ProdSize PRIMARY KEY (id_prod, id_size),
	CONSTRAINT FK_ProdSize_prod FOREIGN KEY (id_prod) REFERENCES Products (id_prod),
	CONSTRAINT FK_ProdSize_size FOREIGN KEY (id_size) REFERENCES Sizes (id_size),
)

CREATE TABLE ProdColor (
    id_prodColor UNIQUEIDENTIFIER DEFAULT NEWID() UNIQUE,
	id_prod UNIQUEIDENTIFIER NOT NULL,
	id_color INT NOT NULL,
	CONSTRAINT PK_ProdColor PRIMARY KEY (id_prod, id_color),
	CONSTRAINT FK_ProdColor_prod FOREIGN KEY (id_prod) REFERENCES Products (id_prod),
	CONSTRAINT FK_ProdColor_color FOREIGN KEY (id_color) REFERENCES Colors (id_color),
)

CREATE TABLE ProdColorImages (
    id_prodColorImage UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    id_prodColor UNIQUEIDENTIFIER NOT NULL,
    img_URL NVARCHAR(1000) NOT NULL,
    CONSTRAINT FK_ProdColorImages_prodColor FOREIGN KEY (id_prodColor) REFERENCES ProdColor(id_prodColor)
);


CREATE TABLE ProdMaterial (
	id_prod UNIQUEIDENTIFIER NOT NULL,
	id_material INT NOT NULL,
	percentage_mat DECIMAL (5,2) NOT NULL,
	CONSTRAINT PK_ProdMat PRIMARY KEY (id_prod, id_material),
	CONSTRAINT CK_percentage CHECK (percentage_mat BETWEEN 0 AND 100),
	CONSTRAINT FK_ProdMat_prod FOREIGN KEY (id_prod) REFERENCES Products (id_prod),
	CONSTRAINT FK_ProdMat_Mat FOREIGN KEY (id_material) REFERENCES Materials (id_material),
)


CREATE TABLE Orders (
	id_order INT IDENTITY (1,1) PRIMARY KEY,
	id_user UNIQUEIDENTIFIER NOT NULL,
	date_order DATETIME DEFAULT GETDATE(),
	total DECIMAL(10,2) not null,
	stateOrder NVARCHAR(50) NOT NULL,
	CONSTRAINT CK_totale CHECK (total >= 0),
	CONSTRAINT CK_stato CHECK (stateOrder IN ('in elaborazione','spedito','consegnato')),
	CONSTRAINT FK_Ordini_Utenti FOREIGN KEY (id_user) REFERENCES Users (id_user),
)

CREATE TABLE Cart (
	id_order INT IDENTITY (1,1) NOT NULL,
	id_prod UNIQUEIDENTIFIER NOT NULL,
	qt INT NOT NULL,
	price DECIMAL (10,2) NOT NULL,
	CONSTRAINT PK_ordineprodotto PRIMARY KEY (id_order, id_prod), 
	CONSTRAINT CK_quantita CHECK (qt >0),
	CONSTRAINT CK_prezzo_unitario CHECK (price >0),
	CONSTRAINT FK_OrdineProdotto_Ordine FOREIGN KEY (id_order) REFERENCES Orders (id_order),
	CONSTRAINT FK_OrdineProdotto_Prodotto FOREIGN KEY (id_prod) REFERENCES Products (id_prod) 
)

CREATE TABLE Ratings(
	id_rating UNIQUEIDENTIFIER PRIMARY KEY,
	id_prod UNIQUEIDENTIFIER NOT NULL,
	id_user UNIQUEIDENTIFIER NOT NULL,
	comment NVARCHAR(500), 
	rating INT NOT NULL,
	CreatedAt DATETIME DEFAULT GETDATE(),
	CONSTRAINT CK_rating CHECK (rating BETWEEN 1 AND 5),
	CONSTRAINT FK_Rating_Prodotto FOREIGN KEY (id_prod) REFERENCES Products (id_prod),
	CONSTRAINT FK_Rating_Users FOREIGN KEY (id_user) REFERENCES Users (id_user), 
)

INSERT INTO Sizes (nome) VALUES  
('XS'), 
('S'), 
('M'), 
('L'), 
('XL');

INSERT INTO Colors (nome) VALUES  
('Black'), 
('White'), 
('Pink'), 
('Red'), 
('Green'), 
('Yellow'), 
('Purple'), 
('Orange'), 
('Brown'), 
('Gold'), 
('Silver'), 
('Multicolor'), 
('Blue');


INSERT INTO Materials (nome) VALUES 
('Cotone'),
('Lino'),
('Lana'),
('Seta'),
('Poliestere'),
('Poliammide'),
('Acrilico'),
('Elastan'),
('Viscosa');

INSERT INTO Category(nome) VALUES 
('Top'),
('Vestiti'),
('Jeans e Pantaloni'),
('Maglieria e felpe'),
('Gonne'),
('Soprabiti'),
('Pantaloncini'),
('Pezzi interi');

SELECT * FROM Products


-- Categoria 1: Top
INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) 
VALUES 
  ('ONLY', 'F', 'ONLANTONIA LIFE TEE BOX JRS - T-shirt basic - detail bow', 1, 'Scollo: Tondo / Dettagli: Applicazioni in pietre/ Avvertenze: Lavaggio delicato, Non asciugare in asciugatrice, non candeggiare./ Vestibilità: Regolare/ Linea: Diritta/ Lunghezza: Lunghezza normale/ Lunghezza manica: Mezze maniche', 19.99, 100),
  ('ONLY', 'F', 'ONLLIVE LOVE PUFFTOP - T-shirt basic - laurel wreath', 1, 'Scollo: Tondo/ Fantasia: Monocromo/ Avvertenze: Non asciugare in asciugatrice. lavaggio a macchina a 30 gradi, non candeggiare./ Vestibilità:  Slim fit/ Linea: Aderente/ Lunghezza: Lunghezza normale/ Lunghezza manica: Corte', 16.99, 100),
  ('Jack & Jones', 'U', 'JJWOLFIE TEE CREW NECK 5 PACK - T-shirt con stampa', 1, 'Scollo: Tond/, Fantasia: Floreale/ Avvertenze: Lavaggio a macchina a 30 gradi, Non asciugare in asciugatrice, non candeggiare./ Vestibilità: Comoda/ Linea: Diritta/ Lunghezza: Lunghezza normale/ Lunghezza manica: Mezze maniche', 59.99, 30),
  ('Pier One', 'M', 'T-shirt con stampa', 1, 'Scollo: Tondo/ Trasparenza: Leggera/ Fantasia: Stampa/ Avvertenze: Non asciugare in asciugatrice, lavaggio a macchina a 30 gradi./ Vestibilità: Regolare/ Linea: Diritta/ Lunghezza: Lunghezza normale/ Lunghezza manica: Corte', 12.99, 50),
  ('Levi''s', 'F', 'THE PERFECT - T-shirt con stampa', 1, 'Scollo: Tondo/ Fantasia: Floreale/ Dettagli: Paillette /Avvertenze: Lavaggio delicato, Non asciugare in asciugatrice, non candeggiare./ Vestibilità: Regolare/ Linea: Diritta/ Lunghezza: Lunghezza normale/ Lunghezza manica: Corte', 19.99, 100);


 DECLARE @1t UNIQUEIDENTIFIER, @2t UNIQUEIDENTIFIER, @3t UNIQUEIDENTIFIER,@4t UNIQUEIDENTIFIER,@5t UNIQUEIDENTIFIER

  SELECT @1t = id_prod FROM Products WHERE nome = 'ONLANTONIA LIFE TEE BOX JRS - T-shirt basic - detail bow'
  SELECT @2t = id_prod FROM Products WHERE nome = 'ONLLIVE LOVE PUFFTOP - T-shirt basic - laurel wreath'
  SELECT @3t = id_prod FROM Products WHERE nome = 'JJWOLFIE TEE CREW NECK 5 PACK - T-shirt con stampa'
  SELECT @4t = id_prod FROM Products WHERE nome = 'T-shirt con stampa'
  SELECT @5t = id_prod FROM Products WHERE nome = 'THE PERFECT - T-shirt con stampa'

  INSERT INTO ProdColor (id_prod, id_color) VALUES
	(@1t, 1),
	(@1t, 2),
	(@2t, 1),
	(@2t, 2),
	(@2t, 5),
	(@2t, 3),
	(@3t, 9),
	(@4t, 2),
	(@4t, 13),
	(@5t, 1),
	(@5t, 2),
	(@5t, 3),
	(@5t, 4),
	(@5t, 5),
	(@5t, 6);

INSERT INTO ProdColorImages (id_prodColor, img_URL)
SELECT 
    pc.id_prodColor,
    img.img_URL
FROM 
    ProdColor pc
JOIN 
    (VALUES
        (@1t, 1, 'https://img01.ztat.net/article/spp-media-p1/f42fcdb62f0644c9aff6234928b7ca5b/1a69b063727840af8ba2bf669e473a74.jpg?imwidth=1800'),
        (@1t, 2, 'https://img01.ztat.net/article/spp-media-p1/1560a3e01bdf48f5a3846a6fb4560c59/ba6ed2518e1147219058adeb40fa39d1.jpg?imwidth=1800'),
        (@2t, 1, 'https://img01.ztat.net/article/spp-media-p1/238a9afd31f54d27a5cdbbbad63abe4c/4053d1d2da664aa9b694d799f9303c47.jpg?imwidth=1800'),
        (@2t, 2, 'https://img01.ztat.net/article/spp-media-p1/815bfdacc968464688893f1a6220895e/f07c5a938df84bb29dcf427805e67f7b.jpg?imwidth=1800'),
        (@2t, 5, 'https://img01.ztat.net/article/spp-media-p1/b007009736194afa85d69639fdc9411a/df262f95ff054eabbb410d4eb7ca1e98.jpg?imwidth=1800'),
        (@2t, 3, 'https://img01.ztat.net/article/spp-media-p1/0b79015cf1b94336818bb879d4b1a0e7/4a7eb1e8374b45e4b074ea04f4bec6e9.jpg?imwidth=1800'),
        (@3t, 9, 'https://img01.ztat.net/article/spp-media-p1/eaa7eee8692444a385636058ab978607/17c935616d154697a65c714a0985d7d1.jpg?imwidth=1800'),
        (@4t, 2, 'https://img01.ztat.net/article/spp-media-p1/fa1bd71a0f943b87bd598acdeb925c37/5a2bd39e4fd945e989a41ea0b2c47ff1.jpg?imwidth=1800'),
        (@4t, 13, 'https://img01.ztat.net/article/spp-media-p1/59b680833f6432a5963778034c439764/64dcfa08cf144e349da187221da4b0a4.jpg?imwidth=1800'),
        (@5t, 1, 'https://img01.ztat.net/article/spp-media-p1/590be4e4ff7b3644a4f4ea9f265e1df6/1720578bc00c45ffb93e3c98431ba8ae.jpg?imwidth=1800'),
        (@5t, 2, 'https://img01.ztat.net/article/spp-media-p1/b47c16165a8a497fa56da4ceee912f3b/baef479f782c43adbedaf562ebb10ba5.jpg?imwidth=1800'),
        (@5t, 3, 'https://img01.ztat.net/article/spp-media-p1/8af5b023c74443b88445ba9bb3a01262/e106cbe2387c4072bfc91edf0cee085b.jpg?imwidth=1800'),
        (@5t, 4, 'https://img01.ztat.net/article/spp-media-p1/55b27d6389ca4370b8195cbc1541c3d0/77425cae531f492da0b8947773804a69.jpg?imwidth=1800'),
        (@5t, 5, 'https://img01.ztat.net/article/spp-media-p1/b4b81af3b8a64293b5d6121ae66d705c/8d8344cd674e46bd8897e809d85c096b.jpg?imwidth=1800'),
        (@5t, 6, 'https://img01.ztat.net/article/spp-media-p1/faed5b9c715d41e994882987b020befa/0e1b2144f66340028349370a9283a8f1.jpg?imwidth=1800')
    ) AS img(id_prod, id_color, img_URL)
    ON pc.id_prod = img.id_prod 
    AND pc.id_color = img.id_color;
	
INSERT INTO ProdSize (id_prod, id_size) VALUES
  (@1t, 1), (@1t, 2), (@1t, 3), (@1t, 4), (@1t, 5),
  (@2t, 1), (@2t, 2), (@2t, 3), (@2t, 4), (@2t, 5),
  (@3t, 1), (@3t, 2), (@3t, 3), (@3t, 4), (@3t, 5),
  (@4t, 1), (@4t, 2), (@4t, 3), (@4t, 4), (@4t, 5),
  (@5t, 1), (@5t, 2), (@5t, 3), (@5t, 4), (@5t, 5);


  INSERT INTO ProdMaterial (id_prod, id_material, percentage_mat) VALUES
  (@1t, 1, 100.00),
  (@2t, 1, 95.00),
  (@2t, 8, 5.00),
  (@3t, 1, 100.00),
  (@4t, 1, 100.00),
  (@5t, 1, 100.00);

   
-- Categoria 2: Vestiti
INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) 
VALUES 
  ('Lauren Ralph Lauren', 'F', 'JERSEY OFF THE SHOULDER GOWN - Abito da sera', 2,  'Scollo: Incrociato / Dettagli: Sottoveste/ Chiusura: Cerniera / Avvertenze: Lavaggio a macchina a 30 gradi, Lavaggio a secco possibile./ Vestibilità: Slim fit/ Linea: Aderente/ Lunghezza: Lungo/ Lunghezza manica: Molto corte', 279.99, 10),
  ('WAL G.', 'F', 'YANNY HALTER NECK - Abito da sera', 2, 'Scollo: Schiena scoperta / Dettagli: Cucitura sul seno/Colletto: Coreana /Chiusura: Cerniera / Avvertenze: Lavaggio a secco, lavaggio a macchina a 30 gradi./ Vestibilità: Slim fit/ Linea: Aderente/ Lunghezza: Lungo/ Lunghezza manica: Senza maniche', 49.99, 50),
  ('WE Fashion', 'F', 'Vestito casual ', 2, 'Scollo: Incrociato / Dettagli: Sottoveste /Fantasia: Melange /Chiusura: Lacci / Avvertenze: Lavaggio a secco, lavaggio a macchina a 30 gradi./ Vestibilità: Regolare/ Linea: Aderente/ Lunghezza: Sul polpaccio/ Lunghezza manica: Corte', 80.00, 50),
  ('Roberto Cavalli', 'F', 'DRESS - Abito da sera', 2, 'Scollo: Schiena scoperta / Dettagli: Nastro /Fantasia: Stampa /Chiusura: Lacci / Avvertenze: Lavaggio a secco, non candeggiare, Non asciugare in asciugatrice./ Vestibilità: Slim fit/ Linea: Aderente/ Lunghezza: Lungo/ Lunghezza manica: Spalline sottili', 1950.00, 1),
  ('Swing', 'F', 'HEY KYLA - Abito da sera ', 2, 'Scollo: Schiena scoperta / Dettagli: Sottoveste, Applicazioni in pietre, bordatura /Fantasia: Monocromo /Chiusura: Cerniera/ Trasparenza: Forte/ Avvertenze: Lavaggio a secco, non candeggiare, Non asciugare in asciugatrice./ Vestibilità: Regolare/ Linea: Svasata/ Lunghezza: Lungo/ Lunghezza manica: Spalline sottili', 229.99, 60);

   DECLARE @1v UNIQUEIDENTIFIER, @2v UNIQUEIDENTIFIER, @3v UNIQUEIDENTIFIER,@4v UNIQUEIDENTIFIER,@5v UNIQUEIDENTIFIER

  SELECT  @1v = id_prod FROM Products WHERE nome = 'JERSEY OFF THE SHOULDER GOWN - Abito da sera'
  SELECT @2v = id_prod FROM Products WHERE nome = 'YANNY HALTER NECK - Abito da sera'
  SELECT @3v = id_prod FROM Products WHERE nome = 'Vestito casual'
  SELECT @4v = id_prod FROM Products WHERE nome = 'DRESS - Abito da sera'
  SELECT @5v = id_prod FROM Products WHERE nome = 'HEY KYLA - Abito da sera '

INSERT INTO ProdColor (id_prod, id_color) 
VALUES
(@1v, 2),
(@1v, 1),
(@1v, 13),
(@1v, 5),
(@1v, 3),
(@2v, 5),
(@2v, 4),
(@2v, 13),
(@3v, 10),
(@3v, 13),
(@4v, 4),
(@5v, 5),
(@5v, 4);

INSERT INTO ProdColorImages (id_prodColor, img_URL)
SELECT 
    pc.id_prodColor,
    img.img_URL
FROM 
    ProdColor pc
JOIN 
    (VALUES
        (@1v, 2, 'https://img01.ztat.net/article/spp-media-p1/df77400696784f35818dc8f29eff3df6/ad69cead60f94b60b7c615eac5b4052f.jpg?imwidth=1800'),
        (@1v, 1, 'https://img01.ztat.net/article/spp-media-p1/3ebd6c6c152d4c319dc6aa3295691fae/baa60038184a461c9abcefe8df258f38.jpg?imwidth=1800'),
        (@1v, 13, 'https://img01.ztat.net/article/spp-media-p1/352d8937eca947429b8a75bcc397d58d/c55961323dba46da8aa490dadb26423c.jpg?imwidth=1800&filter=packshot'),
        (@1v, 5, 'https://img01.ztat.net/article/spp-media-p1/34a85b8e3f8c409caa00fc8f61483716/ae697d0c0a13437d9a3adaaa3b463be8.jpg?imwidth=1800'),
        (@1v, 3, 'https://img01.ztat.net/article/spp-media-p1/816ce4fa5fda44eabd32b8ee11691865/fa6fa7a896f64d139ef7e52830d17ed0.jpg?imwidth=1800'),
        
        (@2v, 5, 'https://img01.ztat.net/article/spp-media-p1/90e55bd4c25440b292e5abed28c4a2f4/047dad1566ae4e81864f90ebf919326c.jpg?imwidth=1800'),
        (@2v, 4, 'https://img01.ztat.net/article/spp-media-p1/9a42f76be7604db99282c87289566cda/7d2387ee890a4c7fb6e153a19396f91d.jpg?imwidth=1800'),
        (@2v, 13, 'https://img01.ztat.net/article/spp-media-p1/853336d18b434524b818516d3103a941/a6c05056967f4222b4164017dc5c23c5.jpg?imwidth=1800'),
        
        (@3v, 10, 'https://img01.ztat.net/article/spp-media-p1/04017cff315f4426b74e8a69f89670e1/71261d2f7f9b48b4848681293a268ccb.jpg?imwidth=1800'),
        (@3v, 13, 'https://img01.ztat.net/article/spp-media-p1/77c5c62ac45f46c5b10874fa2002b564/2c0e25c0854c4beaa6ae015f15d903f7.jpg?imwidth=1800'),
        
        (@4v, 4, 'https://img01.ztat.net/article/spp-media-p1/41eb159150644d73883a88d0d686c469/58724dc4a0154d13b06836310d7eab50.jpg?imwidth=1800'),
        
        (@5v, 5, 'https://img01.ztat.net/article/spp-media-p1/7487aa3a2d6d466c8aa43131844f0ee9/1caa3503a56849269a9c06751ff8afcd.jpg?imwidth=1800'),
        (@5v, 4, 'https://img01.ztat.net/article/spp-media-p1/9d94b0fdd77f4bfda701e562f09fbe84/0c8131869ee947ec858d20126cf410fb.jpg?imwidth=1800')
    ) AS img(id_prod, id_color, img_URL)
    ON pc.id_prod = img.id_prod 
    AND pc.id_color = img.id_color;

INSERT INTO ProdSize (id_prod, id_size) VALUES
  (@1v, 1), (@1v, 2), (@1v, 3), (@1v, 4), (@1v, 5),
  (@2v, 1), (@2v, 2), (@2v, 3), (@2v, 4), (@2v, 5),
  (@3v, 1), (@3v, 2), (@3v, 3), (@3v, 4), (@3v, 5),
  (@4v, 3),
  (@5v, 2), (@5v, 3), (@5v, 4), (@5v, 5);


  INSERT INTO ProdMaterial (id_prod, id_material, percentage_mat) VALUES
  (@1v, 5, 95.00), (@1v, 8, 5.00),
  (@2v, 5, 95.00), (@2v, 8, 5.00),
  (@3v, 9, 60.00), (@3v, 5, 37.00), (@3v, 8, 2.00),
  (@4v, 4, 100.00),
  (@5v, 5, 100.00);


-- Categoria 3: Jeans e Pantaloni
INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) VALUES 
 ('Levi''s®', 'F', 'XL BALLOON - Jeans baggy', 3,  'Vita: Normale / Chiusura: Cerniera nascosta/ Tasche: Tasche posteriori, Tasche laterali/ Dettagli: Cintura inclusa/ Vestibilità: Ampia/ Linea: Avvolgente/ Lunghezza: Lungo', 129.99, 130),
  ('Imperial', 'F', 'AMPI VITA ALTA - Pantaloni', 3, 'Vita: Con cintura/ Chiusura: Cerniera nascosta /Tasche: Tasche laterali /Dettagli: Cintura inclusa / Vestibilità: Ampia/ Linea: Avvolgente/ Lunghezza: Lungo', 86.00, 90),
  ('Adidas', 'F', 'ADIBREAK - Pantaloni sportivi', 3, 'Vita: Elastici /Chiusura: Con chiusura a scatto/ Tasche: Tasche laterali/Dettagli: Fascia elastica, ricamo/ Vestibilità: Ampia/ Linea: Diritta/ Lunghezza: Lungo', 74.99, 50),
  ('Karl Kani', 'M', 'Jeans baggy', 3, 'Vita: Normale/ Chiusura: Cerniera nascosta/ Tasche: Tasche posteriori, Tasche laterali/ Dettagli: Ricamo/ Vestibilità: Ampia/ Linea: Svasata/ Lunghezza: Lungo', 99.95, 40),
  ('Adidas', 'M', 'Pantaloni sportivi', 3, 'Vita: Normale/ Tasche: Tasche laterali/ Fantasia: Stampa/ Dettagli: Fascia elastica/ Vestibilità: Regolare/ Linea: Diritta/ Lunghezza: Lungo', 69.99, 100);

   DECLARE @1j UNIQUEIDENTIFIER, @2j UNIQUEIDENTIFIER, @3j UNIQUEIDENTIFIER,@4j UNIQUEIDENTIFIER,@5j UNIQUEIDENTIFIER

  SELECT @1j = id_prod FROM Products WHERE nome = 'XL BALLOON - Jeans baggy'
  SELECT @2j = id_prod FROM Products WHERE nome = 'AMPI VITA ALTA - Pantaloni'
  SELECT @3j = id_prod FROM Products WHERE nome = 'ADIBREAK - Pantaloni sportivi'
  SELECT @4j = id_prod FROM Products WHERE nome = 'Jeans baggy'
  SELECT @5j = id_prod FROM Products WHERE nome = 'Pantaloni sportivi'

INSERT INTO ProdColor (id_prod, id_color) 
VALUES
(@1j, 13),
(@1j, 1),
(@2j, 3),
(@2j, 11),
(@3j, 1),
(@3j, 3),
(@3j, 4),
(@3j, 5),
(@3j, 6),
(@3j, 7),
(@4j, 11),
(@5j, 2),
(@5j, 13),
(@5j, 1);

 INSERT INTO ProdColorImages (id_prodColor, img_URL)
SELECT 
    pc.id_prodColor,
    img.img_URL
FROM 
    ProdColor pc
JOIN 
    (VALUES
        (@1j, 13, 'https://img01.ztat.net/article/spp-media-p1/0c25ca79030f47a7b4204792a1b48104/aa6b09e76f7d4012a81f5fbc0a1b23d3.jpg?imwidth=1800'),
        (@1j, 1, 'https://img01.ztat.net/article/spp-media-p1/77ec5d49fce04c33b876f282500dc16a/e3bc073e10b94132a1da401d3e211192.jpg?imwidth=1800'),
        
        (@2j, 3, 'https://img01.ztat.net/article/spp-media-p1/db4b1b800f0a404b87a3b17c307c1ce1/e1fce129e9374ef5b932daacd72281a3.jpg?imwidth=1800'),
        (@2j, 11, 'https://img01.ztat.net/article/spp-media-p1/efe192e727b947a9a91d7cf1518b2e2b/33c6e606961a425eb76ba1a7f1d4f2a3.jpg?imwidth=1800'),
        
        (@3j, 1, 'https://img01.ztat.net/article/spp-media-p1/dbe0fe36018a4389b9f50f1a707726f6/74149142fdce4d82a9c909e10fd7a36d.jpg?imwidth=1800'),
        (@3j, 3, 'https://img01.ztat.net/article/spp-media-p1/7b42c884ef354f34b580e0840c81d6b7/764d9d778ad647f895a5de25e8200ed8.jpg?imwidth=1800'),
        (@3j, 4, 'https://img01.ztat.net/article/spp-media-p1/beebb37f348a4b5abc12d4bf648f304e/7586f168ff1548139bda843d74bda4c9.jpg?imwidth=1800'),
        (@3j, 5, 'https://img01.ztat.net/article/spp-media-p1/43461e1bc7ac4d53a4ca471d9abe7b23/f56944593a474c7c93b72f3797649d76.jpg?imwidth=1800'),
        (@3j, 6, 'https://img01.ztat.net/article/spp-media-p1/7594ac950b67410e97f6b84876aae8db/3f7db5a864684325a791eb4d0ce92923.jpg?imwidth=1800'),
        (@3j, 7, 'https://img01.ztat.net/article/spp-media-p1/c7b9b7b18ce2487f94716577df5858e1/da04ea6afe05414baf67900148546bc3.jpg?imwidth=1800'),
        
        (@4j, 11, 'https://img01.ztat.net/article/spp-media-p1/5ebed6f552ba468397205626f74667c1/870efb3635c041a9844c9121f7ab3ceb.jpg?imwidth=1800'),
        
        (@5j, 2, 'https://img01.ztat.net/article/spp-media-p1/deba57b6e4ba4eabb8ba46bcda9f5585/843b3d4f472f4606ac929fc963443fb5.jpg?imwidth=1800'),
        (@5j, 13, 'https://img01.ztat.net/article/spp-media-p1/246d667f36704cac8298cd2ce433cba7/f34db594b2e54927b4dde4a8da61a8ab.jpg?imwidth=1800'),
        (@5j, 1, 'https://img01.ztat.net/article/spp-media-p1/8f708edeb3a34badba367e947508845e/7eeef7b33ae24334815d4a9763e53218.jpg?imwidth=1800')
    ) AS img(id_prod, id_color, img_URL)
    ON pc.id_prod = img.id_prod 
    AND pc.id_color = img.id_color;


INSERT INTO ProdSize (id_prod, id_size) VALUES
  (@1j, 2), (@1j, 3), (@1j, 4),
  (@2j, 1), (@2j, 2), (@2j, 3), (@2j, 4), (@2j, 5),
  (@3j, 1), (@3j, 2), (@3j, 3), (@3j, 4), (@3j, 5),
  (@4j, 1), (@4j, 2), (@4j, 3), (@4j, 4), (@4j, 5),
  (@5j, 2), (@5j, 3), (@5j, 4), (@5j, 5);


  INSERT INTO ProdMaterial (id_prod, id_material, percentage_mat) VALUES
  (@1j, 1, 100.00),
  (@2j, 5, 96.00), (@2j, 8, 5.00),
  (@3j, 5, 100.00),
  (@4j, 1, 100.00),
  (@5j, 5, 100.00);

-- Categoria 4: Maglieria e Felpe
INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) VALUES 
  ('Guess', 'F', 'Cardigan - himmelblau', 4, 'Colletto: Collo a scialle / Tasche: Tasche anteriori/ Fantasia: Monocromo/ Vestibilità: Oversize /Linea: Diritta/Lunghezza: Extra lungo/Lunghezza manica: Lunghe', 160.00, 50),
  ('PULL&BEAR', 'F', 'Maglione', 4, 'Scollo: Smanicato/ Fantasia: Melange/ Vestibilità: Regolare/ Linea: Diritta/ Lunghezza: Lunghezza normale/ Lunghezza manica: Lunghe' , 29.99, 150),
  ('Stradivarius', 'F', 'FELTED V-NECK - Maglione', 4, 'Scollo: A V profondo/ Fantasia: Monocromo/ Vestibilità: Regolare/ Linea: Diritta/ Lunghezza: Lunghezza normale/ Lunghezza manica: Lunghe', 29.99, 100),
  ('YOURTURN', 'U', 'UNISEX - Felpa con cappuccio', 4, 'Colletto: Cappuccio/ Chiusura: Corda/ Tasche: Tasca a marsupio/Fantasia: Stampa /Dettagli: Fascia elastica/ Vestibilità: Comoda/ Linea: Diritta / Lunghezza: Lungo/ Lunghezza manica: Lunghe/ Avvertenze: Lavaggio a macchina a 30 gradi, lavaggio delicato, Non asciugare in asciugatrice', 44.99, 80),
  ('Upscale by Mister Tee', 'M', 'HEAVY OVERSIZE - Felpa con cappuccio', 4, 'Colletto: Cappuccio/ Tasche: Tasca a marsupio/ Fantasia: Stampa/ Dettagli: Fascia elastica/ Vestibilità: Oversize/ Linea: Diritta/ Lunghezza: Lungo/ Lunghezza manica: Lunghe', 59.99, 70);

   DECLARE @1m UNIQUEIDENTIFIER, @2m UNIQUEIDENTIFIER, @3m UNIQUEIDENTIFIER,@4m UNIQUEIDENTIFIER,@5m UNIQUEIDENTIFIER

  SELECT @1m = id_prod FROM Products WHERE nome = 'Cardigan - himmelblau'
  SELECT @2m = id_prod FROM Products WHERE nome = 'Maglione'
  SELECT @3m = id_prod FROM Products WHERE nome = 'FELTED V-NECK - Maglione'
  SELECT @4m = id_prod FROM Products WHERE nome = 'UNISEX - Felpa con cappuccio'
  SELECT @5m = id_prod FROM Products WHERE nome = 'HEAVY OVERSIZE - Felpa con cappuccio'

INSERT INTO ProdColor (id_prod, id_color) 
VALUES
(@1m, 13),
(@1m, 1),
(@1m, 2),

(@2m, 9),
(@2m, 1),

(@3m, 6),
(@3m, 9),
(@3m, 5),
(@3m, 11),
(@3m, 3),

(@4m, 13),
(@4m, 1),
(@4m, 9),
(@4m, 2),
(@4m, 3),
(@4m, 5),
(@4m, 10),

(@5m, 1),
(@5m, 2),
(@5m, 8);

INSERT INTO ProdColorImages (id_prodColor, img_URL)
SELECT 
    pc.id_prodColor,
    img.img_URL
FROM 
    ProdColor pc
JOIN 
    (VALUES
        (@1m, 13, 'https://img01.ztat.net/article/spp-media-p1/dca393197ac74bafbd4ba86960a6a606/42e3097ed2fb4dc189cf56ef24a0999c.jpg?imwidth=1800'),
        (@1m, 1, 'https://img01.ztat.net/article/spp-media-p1/3848887e74804f4fb628874ccbfe4f44/cd0b7d5f9c964631969d721329b1f245.jpg?imwidth=1800'),
        (@1m, 2, 'https://img01.ztat.net/article/spp-media-p1/2aac40167e054c41b7aa151c59dfba01/552904d6595342ffac8bc5057743dada.jpg?imwidth=1800'),

        (@2m, 9, 'https://img01.ztat.net/article/spp-media-p1/b696a3a05b02499480af607b7ac54bb3/7d01e54cf50c42f9b7312e52c06db22f.jpg?imwidth=1800'),
        (@2m, 1, 'https://img01.ztat.net/article/spp-media-p1/d5fc0c9248fd492b9fedd7be136b73e2/3d7305af864b48dcb3dcf6336c51d9b0.jpg?imwidth=1800'),

        (@3m, 6, 'https://img01.ztat.net/article/spp-media-p1/394526c3f9d4413da06c2a6644df5418/74cef7094a7f4ff197e1d07e3fe3e1e0.jpg?imwidth=1800'),
        (@3m, 9, 'https://img01.ztat.net/article/spp-media-p1/7b42c884ef354f34b580e0840c81d6b7/764d9d778ad647f895a5de25e8200ed8.jpg?imwidth=1800'),
        (@3m, 5, 'https://img01.ztat.net/article/spp-media-p1/bfec421b29a344c7aaf03f5c7c368a46/dab4b7ea2ba54b24abbbb0e84c7cdc6e.jpg?imwidth=1800'),
        (@3m, 11, 'https://img01.ztat.net/article/spp-media-p1/3055b43bf71546aa9891e7383f7a8a63/2a4f0880f5f4476aa0448030db29e4fb.jpg?imwidth=1800'),
        (@3m, 3, 'https://img01.ztat.net/article/spp-media-p1/9fd19f0a91cb41c69b520664f974332b/0937528670c74c1993da1fe386c82411.jpg?imwidth=1800'),

        (@4m, 13, 'https://img01.ztat.net/article/spp-media-p1/4e2617982ae34f2b8eea7b9ecc9c8e85/95f8707e2a894c27af76e17f5403feb3.jpg?imwidth=1800'),
        (@4m, 1, 'https://img01.ztat.net/article/spp-media-p1/7b932e2e82fa44d59bedefcb2a63316f/f1f0e004c44c4c0f9eef4bff7e52c23a.jpg?imwidth=1800'),
        (@4m, 9, 'https://img01.ztat.net/article/spp-media-p1/382c281a522a47a0a052e6a2f69ecfc7/520fa9289b5b48838eafeb1d6aad7725.jpg?imwidth=1800'),
        (@4m, 2, 'https://img01.ztat.net/article/spp-media-p1/13847dccd115463bad8109b6ccd75342/ef329fabf61949abbabef7ddf7762b0d.jpg?imwidth=1800'),
        (@4m, 3, 'https://img01.ztat.net/article/spp-media-p1/dfc6d49cef8c4abc9321369cd7d4d243/993a7933fa814600a48a0680f808e9b9.jpg?imwidth=762'),
        (@4m, 5, 'https://img01.ztat.net/article/spp-media-p1/932c51a6dcb1445a9fe2a73c806e2875/f6a545a340504b19919aa1281a10da9e.jpg?imwidth=1800'),
        (@4m, 10, 'https://img01.ztat.net/article/spp-media-p1/2b216bd819dc4b359e3e64a349f72a20/b8d668d0122940eeb2989ff050dfe0dc.jpg?imwidth=1800'),

        (@5m, 1, 'https://img01.ztat.net/article/spp-media-p1/035973a5fda54cb8b4877cba92fbc2ab/9668eb146722481fb788ddcef7a7a4fc.jpg?imwidth=1800'),
        (@5m, 2, 'https://img01.ztat.net/article/spp-media-p1/b165f720e0e94342a2f78d9fb282d2a3/cfc25219207a469abde836b665372759.jpg?imwidth=1800'),
        (@5m, 8, 'https://img01.ztat.net/article/spp-media-p1/eb0b4c0980024b1181e1aece3f6d319b/db69148affab464cb10643bc6a079562.jpg?imwidth=1800')
    ) AS img(id_prod, id_color, img_URL)
    ON pc.id_prod = img.id_prod 
    AND pc.id_color = img.id_color;

INSERT INTO ProdSize (id_prod, id_size) VALUES
  (@1m, 2), (@1m, 3), (@1m, 4),
  (@2m, 1), (@2m, 2), (@2m, 3), (@2m, 4), (@2m, 5),
  (@3m, 1), (@3m, 2), (@3m, 3), (@3m, 4), (@3m, 5),
  (@4m, 1), (@4m, 2), (@4m, 3), (@4m, 4), (@4m, 5),
  (@5m, 2), (@5m, 3), (@5m, 4), (@5m, 5);


  INSERT INTO ProdMaterial (id_prod, id_material, percentage_mat) VALUES
  (@1m, 5, 95.00),(@1m, 6, 5.00),
  (@2m, 7, 80.00), (@2m, 5, 20.00),
  (@3m, 5, 72.00), (@3m, 6, 22.00), (@3m, 7, 4.00), (@3m, 3, 2.00),
  (@4m, 1, 65.00), (@4m, 5, 35.00),
  (@5m, 1, 100.00);


-- Categoria 5: Gonne
  INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) 
VALUES 
  ('Calliope', 'F', 'UNITA - Gonna a campana', 6, 'Chiusura: Cerniera/ Fantasia: Monocromo/ Avvertenze: Lavaggio a macchina a 30 gradi/ Vestibilità: Regolare/ Linea: Dritta/ Lunghezza: Extra corto', 69.99, 70),
  ('Stradivarius', 'F', 'Stradivarius - Minigonna', 6, 'Chiusura: Cerniera/ Fantasia: Monocromo/ Avvertenze: Lavaggio a macchina a 30 gradi/ Vestibilità: Regolare/ Linea: Dritta/ Lunghezza: Extra corto', 59.90, 60),
  ('Guess', 'F', 'LANGER DRAPIERTER - Gonna lunga', 6, 'Vita: Normale/ Fantasia: Monocromo/ Avvertenze: Lavaggio a mano/ Vestibilità:  Regolare/ Linea: Aderente/ Lunghezza: Lungo/', 89.99, 30),
  ('Calliope', 'F', 'Calliope - Gonna a pieghe ', 6, 'Chiusura: Lacci/ Fantasia: Monocromatico/ Avvertenze: Lavaggio a macchina a 30 gradi/ Vestibilità:  Regolare/ Linea: Svasata/ Lunghezza: Corto', 109.99, 30),
  ('Moschino', 'F', 'MOSCHINO - Gonna a campana - white', 6, 'Chiusura: Cerniera/ Fantasia: Monocromo/ Avvertenze: Lavaggio a macchina a 30 gradi, non candeggiare, Non asciugare in asciugatrice/ Vestibilità:  Regolare/ Linea: Svasata/ Lunghezza: Al polpaccio', 999.99, 15);

DECLARE @1sk UNIQUEIDENTIFIER, @2sk UNIQUEIDENTIFIER, @3sk UNIQUEIDENTIFIER, @4sk UNIQUEIDENTIFIER, @5sk UNIQUEIDENTIFIER

SELECT @1sk = id_prod FROM Products WHERE nome = 'UNITA - Gonna a campana'
SELECT @2sk = id_prod FROM Products WHERE nome = 'Stradivarius - Minigonna'
SELECT @3sk = id_prod FROM Products WHERE nome = 'LANGER DRAPIERTER - Gonna lunga'
SELECT @4sk = id_prod FROM Products WHERE nome = 'Calliope - Gonna a pieghe'
SELECT @5sk = id_prod FROM Products WHERE nome = 'MOSCHINO - Gonna a campana - white'

INSERT INTO ProdColor (id_color, id_prod) VALUES 
  (1, @1sk),   (13, @1sk),   (5, @1sk),  (7, @1sk), 
  (1, @2sk),   (11, @2sk), 
  (5, @3sk),   (9, @3sk),   (1, @3sk),   
  (1, @4sk),   (13, @4sk), 
  (2, @5sk);


INSERT INTO ProdColorImages (id_prodColor, img_URL)
SELECT 
    pc.id_prodColor,
    img.img_URL
FROM 
    ProdColor pc
JOIN  
(VALUES
  (1, @1sk, 'https://img01.ztat.net/article/spp-media-p1/3a3fcb1a78544ceea57333438fbe7762/3538389b8063457fac9107ce4f619163.jpg?imwidth=156&filter=packshot'), 
  (13, @1sk, 'https://img01.ztat.net/article/spp-media-p1/9dc3d1d84eaf48d391f296cbb555f0e9/dd3a47a9dbcb47fca4f1f33e1abce8c2.jpg?imwidth=156&filter=packshot'), 
  (5, @1sk, 'https://img01.ztat.net/article/spp-media-p1/6747c66774794a8c9b39a5118c5ea3e7/e4606c2675ac4bec8c7d01246b3ada75.jpg?imwidth=156&filter=packshot'), 
  (7, @1sk, 'https://img01.ztat.net/article/spp-media-p1/613c7232b0654f2d930521bb8ee9ad16/c329a669b77e4b2c8b3bb1eaf450771e.jpg?imwidth=156&filter=packshot'), 

  (1, @2sk, 'https://img01.ztat.net/article/spp-media-p1/e6adba289b2c42ee97e767056fa0ecdf/aee5f4d5c20b486c833ad2b0974882f2.jpg?imwidth=156'), 
  (11, @2sk, 'https://img01.ztat.net/article/spp-media-p1/cd804bc7f77a49fb8610fa2172b5524f/c3efdc5b580949e98f08f1d6195903e9.jpg?imwidth=156'), 

  (5, @3sk, 'https://img01.ztat.net/article/spp-media-p1/689a2e59b08c4b0d909a61cdefe905a8/e642a8ab19904eb7adcf3cab3347d8cf.jpg?imwidth=156'), 
  (9, @3sk, 'https://img01.ztat.net/article/spp-media-p1/d4899e12cece4e32be1998563f364a42/7eea58a84a4d4343997669e25dda3026.jpg?imwidth=156'), 
  (1, @3sk, 'https://img01.ztat.net/article/spp-media-p1/95fc9114eb8944f59a54417589f3f008/cf7964de0bfe444284d75cb7c853efbe.jpg?imwidth=156'), 
  
  (1, @4sk, 'https://img01.ztat.net/article/spp-media-p1/375cab725a644b9d98bdba3e8cf1df20/95d4ea2c8397483880d65905278135dd.jpg?imwidth=156&filter=packshot'), 
  (13, @4sk, 'https://img01.ztat.net/article/spp-media-p1/008e99fe45924c93b4b3201dd42f4083/2c94b08ecf594cbfb0e74111eb00d6f0.jpg?imwidth=156&filter=packshot'), 

  (2, @5sk, 'https://img01.ztat.net/article/spp-media-p1/51a5581ea1714374ac07a721e2d40f9f/266b18ed97724ad183ed0323de9d2a19.jpg?imwidth=762')
      ) AS img(id_color, id_prod, img_URL)
    ON pc.id_prod = img.id_prod 
    AND pc.id_color = img.id_color;
    

INSERT INTO ProdSize (id_prod, id_size) VALUES
  (@1sk, 1),  (@1sk, 2),   (@1sk, 5),
  (@2sk, 1),  (@2sk, 2),  (@2sk, 3),  (@2sk, 4),  (@2sk, 5),
  (@3sk, 1),  (@3sk, 3),  (@3sk, 4),  
  (@4sk, 1),  (@4sk, 2),  (@4sk, 3),  (@4sk, 4),  (@4sk, 5),
  (@5sk, 1),  (@5sk, 2),  (@5sk, 3),  (@5sk, 4),  (@5sk, 5)

INSERT INTO ProdMaterial (id_prod, id_material, percentage_mat) VALUES
  (@1sk, 5, 100.00), 
  (@2sk, 1, 99.00),  (@2sk, 8, 1.00),
  (@3sk, 1, 73.00),  (@3sk, 5, 27.00),
  (@4sk, 5, 90.00),  (@4sk, 8, 10.00),
  (@5sk, 1, 94.00), (@5sk, 8, 6.00)


-- Categoria 6: Soprabiti
INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) 
VALUES 
  ('Emporio Armani', 'M', 'Armani - Trench', 6, 'Colletto: Classico/ Chiusura: Bottoni/ Fantasia: Monocromo/ Avvertenze: Pulizia speciale per la pelle/ Vestibilità: Regolare/ Linea: Aderente/ Lunghezza: Sul polpaccio', 3900.00, 10),
  ('Pier One', 'M', 'Pier One - Trench', 6, 'Colletto: Bavero/ Chiusura: Bottoni/ Fantasia: Monocromo/ Avvertenze: Lavare in lavatrice seguendo le istruzioni sull''etichetta/ Vestibilità: Regolare/ Linea: Aderente/ Lunghezza: Sulla coscia', 69.90, 80),
  ('Versace', 'M', 'Versace Jeans Couture - Trench', 6, 'Colletto: Bavero/ Chiusura: Bottoni/ Fantasia: Monocromo/ Avvertenze: Lavaggio a secco, Non asciugare in asciugatrice/ Vestibilità:  Regolare/ Linea: Dritta/ Lunghezza: Al ginocchio/', 649.99, 40),
  ('Calvin Klein Jeans', 'U', 'EXCLUSIVE LONG OVERCOAT UNISEX - Cappotto classico', 6, 'Colletto: Classico/ Chiusura: Bottoni/ Fantasia: Monocromatico/ Avvertenze: Lavaggio a macchina a 30 gradi/ Vestibilità:  Regolare/ Linea: Aderente/ Lunghezza: Al ginocchio', 399.99, 30),
  ('Ralph Lauren', 'F', 'Lauren Ralph Lauren - Trench', 6, 'Colletto: Classico/ Chiusura: Bottoni/ Fantasia: Monocromo/ Avvertenze: Lavaggio a macchina a 30 gradi/ Vestibilità:  Regolare/ Linea: Aderente/ Lunghezza: Al ginocchio', 349.99, 60);

DECLARE @1g UNIQUEIDENTIFIER, @2g UNIQUEIDENTIFIER, @3g UNIQUEIDENTIFIER, @4g UNIQUEIDENTIFIER, @5g UNIQUEIDENTIFIER

SELECT @1g = id_prod FROM Products WHERE nome = 'Armani - Trench'
SELECT @2g = id_prod FROM Products WHERE nome = 'Pier One - Trench'
SELECT @3g = id_prod FROM Products WHERE nome = 'Versace Jeans Couture - Trench'
SELECT @4g = id_prod FROM Products WHERE nome = 'EXCLUSIVE LONG OVERCOAT UNISEX - Cappotto classico'
SELECT @5g = id_prod FROM Products WHERE nome = 'Lauren Ralph Lauren - Trench'

INSERT INTO ProdColor (id_prod, id_color)
VALUES
  (@1g, 9),
  (@2g, 1),(@2g, 5),(@2g, 13),(@2g, 2),
  (@3g,1),
  (@4g,1),
  (@5g,1),(@5g,2),(@5g,13),(@5g,10)


  INSERT INTO ProdColorImages (id_prodColor, img_URL)
SELECT 
    pc.id_prodColor,
    img.img_URL
FROM 
    ProdColor pc
JOIN 
    (VALUES
        (@1g, 9, 'https://img01.ztat.net/article/spp-media-p1/2bb07039577a4ae38c8bce8ef5807b71/339ba472c8ef4baca54e5d3a382682ed.jpg?imwidth=762'),
        (@2g, 1, 'https://img01.ztat.net/article/spp-media-p1/03748667c284411d8c9655e5cda6b8db/7e14dd9eb63943a6ac0f09f4cf3fd080.jpg?imwidth=156'),
        (@2g, 5, 'https://img01.ztat.net/article/spp-media-p1/11d08ebc00f54d248b90c97b64ea1318/38457c6884334276ad1ee6e26e79f798.jpg?imwidth=156'),
        (@2g, 13, 'https://img01.ztat.net/article/spp-media-p1/357deb031e104714b38350268aa218ed/d8cd16a0afd64b11914ae7dba65bfd33.jpg?imwidth=156'),
        (@2g, 2,  'https://img01.ztat.net/article/spp-media-p1/1498708cb13247e8aba6548d1f61cb6c/bb6161c74bc7458d96dae3a24d5fce03.jpg?imwidth=156'),
        (@3g,1, 'https://img01.ztat.net/article/spp-media-p1/1167dc858ed2474eaef917dcf720fbcb/385846db17f247b69ef5b6f561656be3.jpg?imwidth=156'),
        (@4g,1, 'https://img01.ztat.net/article/spp-media-p1/767c00e6f47748f797d9f63d7ee1539a/c23aaff71566412fba6790696d47a33c.jpg?imwidth=156'),
        (@5g,1, 'https://img01.ztat.net/article/spp-media-p1/f7533a57a1ae44a198922e2a2b73b5b5/6deff79992cf452eb9f79e2064d60af8.jpg?imwidth=156'),
        (@5g,2, 'https://img01.ztat.net/article/spp-media-p1/90dc977e2c6f45f2b01b298a1337304d/d4dda8666523423f864447ba75f14924.jpg?imwidth=156'),
        (@5g,13, 'https://img01.ztat.net/article/spp-media-p1/a92ae3176e52473c9e632f9b4daa864e/d00219b6856446fba2eb4a0bc1f24cd6.jpg?imwidth=156'),
        (@5g,10, 'https://img01.ztat.net/article/spp-media-p1/a4d1ec17ee504b6dbe5d6c876184db44/c2003b175f2547e992f50c56c6bb74f5.jpg?imwidth=156')
    ) AS img(id_prod, id_color, img_URL)
ON pc.id_prod = img.id_prod 
AND pc.id_color = img.id_color;


INSERT INTO ProdSize (id_prod, id_size) VALUES
  (@1g, 1),  (@1g, 2),  (@1g, 4),  (@1g, 5),
  (@2g, 2),  (@2g, 3),  (@2g, 4),  (@2g, 5),
  (@3g, 1),  (@3g, 3),  (@3g, 4),  (@3g, 5),
  (@4g, 1),  (@4g, 2),  (@4g, 3),  (@4g, 4),  (@4g, 5),
  (@5g, 1),  (@5g, 3),  (@5g, 4),  (@5g, 5)

INSERT INTO ProdMaterial (id_prod, id_material, percentage_mat) VALUES
  (@1g, 5, 100.00),
  (@2g, 1, 100.00),
  (@3g, 3, 66.00),
  (@3g, 5, 34.00),
  (@4g, 1, 100.00),
  (@5g, 1, 57.00), (@5g, 5, 43.00)

  -- Categoria 7 
  INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) 
VALUES 
  ('NIKE', 'M', 'DRY PARK III - Pantaloncini sportivi', 7, 'Vita: Elastici/ Fantasia: Monocromo/ Avvertenze: Lavare in lavatrice seguendo le istruzioni sull''etichetta/ Vestibilità: Regolare/ Linea: Diritta/ Lunghezza: Corto', 29.90, 30),
  ('Karl Kani', 'M', 'SMALL SIGNATURE - Shorts', 7, 'Vita: Normale/ Fantasia: Righe/ Avvertenze: Lavare in lavatrice seguendo le istruzioni sull''etichetta/ Vestibilità: Ampia/ Linea: Diritta/ Lunghezza: Al ginocchio', 39.90, 50),
  ('Puma', 'F', 'SHORTS - Pantaloncini sportivi ', 7, 'Viat: Normale, elastici/ Fantasia: Monocromo/ Avvertenze: Lavaggio a macchina a 30 gradi, lavaggio delicato/ Vestibilità:  Regolare/ Linea: Dritta/ Lunghezza: Extra corto/', 19.99, 100),
  ('Stradivarius', 'F', 'MIT RISSEN - Shorts di jeans', 7, 'Vita: Alta/ Fantasia: Animalier/ Avvertenze: Lavaggio a macchina a 30 gradi, Non asciugare in asciugatrice, non candeggiare, lavaggio delicato/ Vestibilità:  Comoda/ Linea: Svasata/ Lunghezza: Extra corto', 79.99, 10),
  ('Jordan', 'U', 'M J DF SPRT DMND - Pantaloncini sportivi', 7, 'Vita: Elastici/ Fantasia: Monocromo/ Avvertenze: Lavaggio a macchina a 30 gradi/ Vestibilità:  Regolare/ Linea: Dritta/ Lunghezza: Corto', 49.99, 20);

DECLARE @1s UNIQUEIDENTIFIER, @2s UNIQUEIDENTIFIER, @3s UNIQUEIDENTIFIER, @4s UNIQUEIDENTIFIER, @5s UNIQUEIDENTIFIER

SELECT @1s = id_prod FROM Products WHERE nome = 'DRY PARK III - Pantaloncini sportivi'
SELECT @2s = id_prod FROM Products WHERE nome = 'SMALL SIGNATURE - Shorts'
SELECT @3s = id_prod FROM Products WHERE nome = 'SHORTS - Pantaloncini sportivi'
SELECT @4s = id_prod FROM Products WHERE nome = 'MIT RISSEN - Shorts di jeans'
SELECT @5s = id_prod FROM Products WHERE nome = 'M J DF SPRT DMND - Pantaloncini sportivi'

INSERT INTO ProdColor (id_prod, id_color)
VALUES
(@1s, 8), (@1s, 1),(@1s, 13),(@1s, 7),
(@2s, 1),
(@3s, 1),(@3s, 2),
(@4s, 13),(@5s, 12);

INSERT INTO ProdColorImages (id_prodColor, img_URL)
SELECT 
    pc.id_prodColor,
    img.img_URL
FROM 
    ProdColor pc
JOIN 
    (VALUES
        (@1s, 8, 'https://img01.ztat.net/article/spp-media-p1/fbc34275901d42a38172c153b268c957/40ed55d2e69d4c47b13fabe4aaa3dcca.jpg?imwidth=156&filter=packshot'),
        (@1s, 1, 'https://img01.ztat.net/article/spp-media-p1/8a3f7b1208f33ea382e945c5abdcf535/6491d363ddee453280698080add78198.jpg?imwidth=156&filter=packshot'),
        (@1s, 13, 'https://img01.ztat.net/article/spp-media-p1/01284039466941389c5965885fdd1235/819b03105f3c4d2ca1cfc849403ea180.jpg?imwidth=156&filter=packshot'),
        (@1s, 7, 'https://img01.ztat.net/article/spp-media-p1/26fe473a25b64f8bba6e8ff3a87155f5/93b42f7b029c4767942dc730874aff4f.jpg?imwidth=156&filter=packshot'),
        (@2s, 1, 'https://img01.ztat.net/article/spp-media-p1/fe7f55adf2154aa5a78db45cc17d1d2f/5b8d456ec4bc4fc5a9bfa75c6177ef4a.jpg?imwidth=156'),
        (@3s, 1, 'https://img01.ztat.net/article/spp-media-p1/34f3817f40a64aa19632686333e51e97/cceaedca70154c5cbb8e05ddb47355ed.jpg?imwidth=156'),
        (@3s, 2, 'https://img01.ztat.net/article/spp-media-p1/1da32f3689094327b3e78a239530d8c1/735c055f962a42d8a2bf8cd5b30ad723.jpg?imwidth=156'),
        (@4s, 13, 'https://img01.ztat.net/article/spp-media-p1/767c00e6f47748f797d9f63d7ee1539a/c23aaff71566412fba6790696d47a33c.jpg?imwidth=156'),
        (@5s, 12, 'https://img01.ztat.net/article/spp-media-p1/8a1f474688894a1d80844ce6b94bfb14/e05ad9bdc28e44cda25e79fefa3ca02b.jpg?imwidth=156')
    ) AS img(id_prod, id_color, img_URL)
    ON pc.id_prod = img.id_prod 
    AND pc.id_color = img.id_color;

	INSERT INTO ProdSize (id_prod, id_size) VALUES
  (@1s, 1),   (@1s, 2),  (@1s, 3),  (@1s, 4),  (@1s, 5),
  (@2s, 2),  (@2s, 3),  (@2s, 5),  (
  @3s, 1),  (@3s, 3),  (@3s, 4),
  (@4s, 1),  (@4s, 2),  (@4s, 3),  (@4s, 4),  (@4s, 5),
  (@5s, 1),  (@5s, 3),  (@5s, 4),  (@5s, 5)


INSERT INTO ProdMaterial (id_prod, id_material, percentage_mat) VALUES
  (@1s, 5, 100.00),
  (@2s, 5, 100.00),
  (@3s, 1, 68.00),   (@3s, 5, 32.00),
  (@4s, 1, 100.00),
  (@5s, 5, 100.00)


  --Categoria 8
  INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) VALUES 
  ('WAL G.', 'F', 'HAVANA - Tuta jumpsuit', 8, 'Scollo: Colletto ampio/ Fantasia: Monocromo/ Avvertenze: Lavare in lavatrice seguendo le istruzioni sull''etichetta/ Vestibilità: Regolare/ Linea: Diritta/ Lunghezza: Lunghezza normale/ Lunghezza manica: Lunghe', 40.90, 100),
  ('Stradivarius', 'F', 'STRAPPY - Tuta jumpsuit', 8, 'Scollo: Ampio/ Fantasia: Monocromo/ Avvertenze: Non asciugare in asciugatrice, lavaggio a macchina a 30 gradi, non candeggiare/ Vestibilità:  Slim fit/ Linea: Aderente/ Lunghezza: Lunghezza normale/ Lunghezza manica: Senza maniche', 54.99, 100),
  ('ONLY', 'F', 'SCHMALE TRÄGER - Tuta jumpsuit', 8, 'Scollo: Schiena scoperta/ Fantasia: Monocromo/ Avvertenze: Lavaggio a macchina a 30 gradi, Non asciugare in asciugatrice, non candeggiare, lavaggio delicato/ Vestibilità:  Regolare/ Linea: Aderente/ Lunghezza: Lunghezza normale/ Lunghezza manica: Spalline sottili', 49.99, 100),
  ('Swing', 'F', 'MIT WEITER IN ANIMAL PRINT - Tuta jumpsuit', 8, 'Scollo: Schiena scoperta/ Fantasia: Animalier/ Avvertenze: Lavaggio a macchina a 30 gradi, Non asciugare in asciugatrice, non candeggiare, lavaggio delicato/ Vestibilità:  Comoda/ Linea: Svasata/ Lunghezza: Lungo/ Lunghezza manica: Senza maniche', 199.99, 100),
  ('WAL G.', 'F', 'MALIA ONE SHOULDER - Tuta jumpsuit', 8, 'Scollo: Schiena scoperta/ Fantasia: Monocromo/ Avvertenze: Lavaggio a macchina a 30 gradi, Non asciugare in asciugatrice, non candeggiare, lavaggio delicato/ Vestibilità:  Regolare/ Linea: Aderente/ Lunghezza: Fino alla caviglia/ Lunghezza manica: Molto corte', 39.99, 100);



DECLARE @1p UNIQUEIDENTIFIER, @2p UNIQUEIDENTIFIER, @3p UNIQUEIDENTIFIER,@4p UNIQUEIDENTIFIER,@5p UNIQUEIDENTIFIER

SELECT @1p = id_prod FROM Products WHERE nome = 'HAVANA - Tuta jumpsuit'
SELECT @2p = id_prod FROM Products WHERE nome = 'STRAPPY - Tuta jumpsuit'
SELECT @3p = id_prod FROM Products WHERE nome = 'SCHMALE TRÄGER - Tuta jumpsuit'
SELECT @4p = id_prod FROM Products WHERE nome = 'MIT WEITER IN ANIMAL PRINT - Tuta jumpsuit'
SELECT @5p = id_prod FROM Products WHERE nome = 'MALIA ONE SHOULDER - Tuta jumpsuit'

INSERT INTO ProdColor (id_prod, id_color)
VALUES
(@1p, 4),(@1p, 1),(@1p, 13),(@1p, 5),
(@2p, 1),(@2p, 2),
(@3p, 1),(@3p, 2),
(@4p, 12),
(@5p, 3);

INSERT INTO ProdColorImages (id_prodColor, img_URL)
SELECT 
    pc.id_prodColor,
    img.img_URL
FROM 
    ProdColor pc
JOIN 
    (VALUES
        (@1p, 4, 'https://img01.ztat.net/article/spp-media-p1/6a129f93120044a08a17d45df050dee6/dfa57a7c5b204b499ed3048e083465f8.jpg?imwidth=156'),
        (@1p, 1, 'https://img01.ztat.net/article/spp-media-p1/7274b03d463f491a876fc7f32c74a280/a48d40320d054e2f9e640d22c328cd22.jpg?imwidth=156'),
        (@1p, 13, 'https://img01.ztat.net/article/spp-media-p1/7274b03d463f491a876fc7f32c74a280/a48d40320d054e2f9e640d22c328cd22.jpg?imwidth=156'),
        (@1p, 5, 'https://img01.ztat.net/article/spp-media-p1/0577b0c9c98848bb9a98fd767e328bb0/e305c3459a334015983bd8a28b1cf70c.jpg?imwidth=156'),

        (@2p, 1, 'https://img01.ztat.net/article/spp-media-p1/5a8c145625444dffb89acfcf2c6ed77e/b0f3e2d07e3c40f1b463e2e8279731f0.jpg?imwidth=156'),
        (@2p, 2, 'https://img01.ztat.net/article/spp-media-p1/16afac3af0264601bc2d5f7673e8abe8/04131f2c783342fa99f9ed0e9589d129.jpg?imwidth=156'),

        (@3p, 1, 'https://img01.ztat.net/article/spp-media-p1/cae3859d5ef042a791e32aa2c327e894/2e7f795587264ade93a1244f46a9bbaa.jpg?imwidth=156'),
        (@3p, 2, 'https://img01.ztat.net/article/spp-media-p1/3605d488b1464d04bb87d365bef39a0d/43d887452b01439fb20dc2dd27393046.jpg?imwidth=156'),

        (@4p, 12, 'https://img01.ztat.net/article/spp-media-p1/f9f59c68a8b3480fbead65883e4cab2c/acb385af4ccb469fa12f5f3c446874a7.jpg?imwidth=156'),

        (@5p, 3, 'https://img01.ztat.net/article/spp-media-p1/14cd47c4adf3414e8545058514b315ea/32ac62ee0484441988e1a710a96a17c4.jpg?imwidth=156')
    ) AS img(id_prod, id_color, img_URL)
    ON pc.id_prod = img.id_prod 
    AND pc.id_color = img.id_color;

INSERT INTO ProdSize (id_prod, id_size) VALUES
  (@1p, 1),  (@1p, 2),  (@1p, 3),  (@1p, 4),  (@1p, 5),
  (@2p, 1),  (@2p, 2),  (@2p, 3),  (@2p, 4),  (@2p, 5),
  (@3p, 2),  (@3p, 3),  (@3p, 5),
  (@4p, 1),  (@4p, 2),  (@4p, 3),  (@4p, 4),  (@4p, 5),
  (@5p, 1),  (@5p, 2),  (@5p, 5);

 INSERT INTO ProdMaterial (id_prod, id_material, percentage_mat) VALUES
  (@1p, 5, 95.00),  (@1p, 8, 5.00),
  (@2p, 9, 83.00),  (@2p, 2, 17.00),
  (@3p, 5, 100.00),
  (@4p, 5, 100.00),
  (@5p, 5, 100.00)



  SELECT * FROM Sizes ORDER BY id_size