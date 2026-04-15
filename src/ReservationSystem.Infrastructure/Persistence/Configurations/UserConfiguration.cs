using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> e)
    {
        e.ToTable("Users");
        e.HasKey(u => u.Id);
        e.Property(u => u.Name).HasMaxLength(100).IsRequired();
        e.HasIndex(u => u.Name).IsUnique();

        var users = new User[Names.Length];
        for (var i = 0; i < Names.Length; i++)
            users[i] = new User { Id = i + 1, Name = Names[i] };
        e.HasData(users);
    }

    private static readonly string[] Names =
    [
        "Anna Kowalska", "Maria Nowak", "Katarzyna Wiśniewska", "Małgorzata Wójcik", "Agnieszka Kowalczyk",
        "Barbara Kamińska", "Ewa Lewandowska", "Elżbieta Zielińska", "Zofia Szymańska", "Teresa Woźniak",
        "Magdalena Dąbrowska", "Joanna Kozłowska", "Janina Mazur", "Krystyna Jankowska", "Danuta Kwiatkowska",
        "Monika Krawczyk", "Beata Kaczmarek", "Marta Piotrowska", "Dorota Grabowska", "Julia Pawłowska",
        "Aleksandra Michalska", "Halina Król", "Jadwiga Wieczorek", "Jolanta Jabłońska", "Grażyna Wróbel",
        "Paulina Nowakowska", "Natalia Majewska", "Karolina Olszewska", "Justyna Stępień", "Edyta Malinowska",
        "Alicja Jaworska", "Urszula Adamczyk", "Iwona Dudek", "Renata Nowicka", "Beata Pawlak",
        "Agata Witkowska", "Aneta Walczak", "Izabela Sikora", "Sylwia Rutkowska", "Patrycja Michalak",
        "Emilia Szewczyk", "Weronika Ostrowska", "Kinga Baran", "Lucyna Tomaszewska", "Janina Zalewska",
        "Bożena Pietrzak", "Wiesława Marciniak", "Marzena Wróblewska", "Gabriela Jasińska", "Bogumiła Zawadzka",

        "Jan Kowalski", "Andrzej Nowak", "Piotr Wiśniewski", "Krzysztof Wójcik", "Stanisław Kowalczyk",
        "Tomasz Kamiński", "Paweł Lewandowski", "Józef Zieliński", "Marcin Szymański", "Marek Woźniak",
        "Michał Dąbrowski", "Grzegorz Kozłowski", "Jerzy Mazur", "Tadeusz Jankowski", "Adam Kwiatkowski",
        "Łukasz Krawczyk", "Zbigniew Kaczmarek", "Ryszard Piotrowski", "Dariusz Grabowski", "Henryk Pawłowski",
        "Mariusz Michalski", "Kazimierz Król", "Wojciech Wieczorek", "Robert Jabłoński", "Mateusz Wróbel",
        "Marian Nowakowski", "Rafał Majewski", "Jacek Olszewski", "Janusz Stępień", "Mirosław Malinowski",
        "Maciej Jaworski", "Sławomir Adamczyk", "Jarosław Dudek", "Kamil Nowicki", "Wiesław Pawlak",
        "Roman Witkowski", "Władysław Walczak", "Jakub Sikora", "Edward Rutkowski", "Mieczysław Michalak",
        "Damian Szewczyk", "Przemysław Ostrowski", "Sebastian Baran", "Arkadiusz Tomaszewski", "Antoni Zalewski",
        "Stefan Pietrzak", "Zygmunt Marciniak", "Igor Wróblewski", "Artur Jasiński", "Witold Zawadzki",

        "Helena Bąk", "Irena Włodarczyk", "Karolina Borkowska", "Marta Czarnecka", "Olga Sawicka",
        "Wanda Sokołowska", "Rozalia Maciejewska", "Kinga Kubiak", "Cecylia Kucharska", "Mariola Wilk",
        "Sabina Wysocka", "Leokadia Lis", "Amelia Szczepańska", "Oliwia Kaźmierczak", "Nadia Andrzejak",
        "Kornelia Przybylska", "Lidia Głowacka", "Malwina Kania", "Daria Mrozowska", "Anita Krajewska",
        "Nikola Urbańska", "Sandra Sadowska", "Adriana Ziółkowska", "Klaudia Sobczak", "Kamila Laskowska",
        "Blanka Kołodziej", "Malina Zakrzewska", "Sara Brzezińska", "Melania Makowska", "Aurelia Borowska",
        "Sonia Kurek", "Diana Markowska", "Eliza Mazurek", "Rozalia Kopała", "Judyta Tomczak",
        "Faustyna Wasilewska", "Marcelina Bielecka", "Michalina Kośmider", "Inga Szymczak", "Kalina Górecka",
        "Liwia Chmielewska", "Bianka Kaczmarczyk", "Gaja Czarnecka", "Iga Kowal", "Nela Bednarek",
        "Kaja Wrona", "Pola Murawska", "Ida Marszałek", "Elena Piekarska", "Róża Sowińska",

        "Błażej Bąk", "Konrad Włodarczyk", "Hubert Borkowski", "Daniel Czarnecki", "Adrian Sawicki",
        "Dominik Sokołowski", "Filip Maciejewski", "Gabriel Kubiak", "Julian Kucharski", "Krystian Wilk",
        "Szymon Wysocki", "Igor Lis", "Wiktor Szczepański", "Oliwier Kaźmierczak", "Alan Andrzejak",
        "Oskar Przybylski", "Cyprian Głowacki", "Gracjan Kania", "Maksymilian Mrozowski", "Nikodem Krajewski",
        "Kajetan Urbański", "Tymon Sadowski", "Bruno Ziółkowski", "Cezary Sobczak", "Fabian Laskowski",
        "Franciszek Kołodziej", "Gustaw Zakrzewski", "Ignacy Brzeziński", "Leon Makowski", "Marcel Borowski",
        "Maurycy Kurek", "Natan Markowski", "Olaf Mazurek", "Patryk Kopała", "Remigiusz Tomczak",
        "Samuel Wasilewski", "Teodor Bielecki", "Tymoteusz Kośmider", "Aleksander Szymczak", "Bartosz Górecki",
        "Dawid Chmielewski", "Eryk Kaczmarczyk", "Gerard Czarnecki", "Ireneusz Kowal", "Janusz Bednarek",
        "Konstanty Wrona", "Ludwik Murawski", "Mikołaj Marszałek", "Norbert Piekarski", "Oktawian Sowiński"
    ];
}
