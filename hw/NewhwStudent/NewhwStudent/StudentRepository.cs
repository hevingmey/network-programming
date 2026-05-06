using System.Collections.Generic;

public class StudentRepository
{
    private List<Student> students = new List<Student>()
    {
        new Student { Id = 1,  Name = "Олексій",  Surname = "Коваленко",   Group = "КН-21" },
        new Student { Id = 2,  Name = "Марія",    Surname = "Іваненко",    Group = "КН-21" },
        new Student { Id = 3,  Name = "Дмитро",   Surname = "Петренко",    Group = "КН-22" },
        new Student { Id = 4,  Name = "Анна",     Surname = "Сидоренко",   Group = "КН-22" },
        new Student { Id = 5,  Name = "Богдан",   Surname = "Бойченко",    Group = "КН-23" },
        new Student { Id = 6,  Name = "Катерина", Surname = "Мельниченко", Group = "КН-23" },
        new Student { Id = 7,  Name = "Іван",     Surname = "Гриценко",    Group = "КН-21" },
        new Student { Id = 8,  Name = "Юлія",     Surname = "Ткаченко",    Group = "КН-22" },
        new Student { Id = 9,  Name = "Сергій",   Surname = "Кравченко",   Group = "КН-23" },
        new Student { Id = 10, Name = "Вікторія", Surname = "Романенко",   Group = "КН-21" },
    };

    private int nextId = 11;

    public List<Student> GetAll()
    {
        return students;
    }

    public Student GetById(int id)
    {
        Student result = null;

        for (int i = 0; i < students.Count; i++)
        {
            if (students[i].Id == id)
            {
                result = students[i];
            }
        }

        return result;
    }

    public List<Student> GetByFilter(string name, string group)
    {
        List<Student> result = new List<Student>();

        for (int i = 0; i < students.Count; i++)
        {
            bool nameOk  = string.IsNullOrEmpty(name)  || students[i].Name.ToLower().Contains(name.ToLower());
            bool groupOk = string.IsNullOrEmpty(group) || students[i].Group.ToLower().Contains(group.ToLower());

            if (nameOk && groupOk)
            {
                result.Add(students[i]);
            }
        }

        return result;
    }

    public Student Add(Student newStudent)
    {
        newStudent.Id = nextId;
        nextId = nextId + 1;

        students.Add(newStudent);

        return newStudent;
    }

    public Student Update(int id, Student updatedData)
    {
        Student found = GetById(id);

        if (found == null)
        {
            return null;
        }

        found.Name    = updatedData.Name;
        found.Surname = updatedData.Surname;
        found.Group   = updatedData.Group;

        return found;
    }
}
